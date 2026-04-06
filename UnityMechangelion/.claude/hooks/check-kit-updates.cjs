#!/usr/bin/env node
// t1k-origin: kit=theonekit-core | repo=The1Studio/theonekit-core | module=null | protected=true
/**
 * check-kit-updates.cjs — Auto-update installed kits/modules at session start.
 *
 * Discovers ALL repos to check from:
 *   1. installedModules (v3) — grouped by repository
 *   2. t1k-config-*.json — each config fragment declares a repo
 * Then for each repo: fetch release manifest or tag, compare, auto-update patch/minor.
 *
 * Self-update guard: skips repos matching CWD's git remote.
 * Cache: 1h TTL (.update-check-cache). Opt-out: features.autoUpdate: false.
 */

const { extractZip } = require('./module-manifest-helpers.cjs');

function isMajorBump(local, remote) {
  return Number(remote.split('.')[0]) > Number((local || '0').split('.')[0]);
}

(async () => {
  try {
    const fs = require('fs');
    const path = require('path');
    const { execSync } = require('child_process');

    const CACHE_TTL_MS = 60 * 60 * 1000;
    const cwd = process.cwd();
    const claudeDir = path.join(cwd, '.claude');
    if (!fs.existsSync(claudeDir)) process.exit(0);

    // Check opt-out flag
    for (const cf of fs.readdirSync(claudeDir).filter(f => f.startsWith('t1k-config-') && f.endsWith('.json'))) {
      try {
        const c = JSON.parse(fs.readFileSync(path.join(claudeDir, cf), 'utf8'));
        if (c.features?.autoUpdate === false) process.exit(0);
      } catch { /* skip */ }
    }

    // Cache check
    const cacheFile = path.join(claudeDir, '.update-check-cache');
    if (fs.existsSync(cacheFile)) {
      if (Date.now() - new Date(fs.readFileSync(cacheFile, 'utf8').trim()).getTime() < CACHE_TTL_MS) process.exit(0);
    }

    const metadataPath = path.join(claudeDir, 'metadata.json');
    if (!fs.existsSync(metadataPath)) process.exit(0);
    let metadata;
    try { metadata = JSON.parse(fs.readFileSync(metadataPath, 'utf8')); } catch { process.exit(0); }

    // Self-update guard: skip repos matching CWD's git remote
    let cwdRemotes = '';
    try { cwdRemotes = execSync('git remote -v 2>/dev/null', { encoding: 'utf8', timeout: 5000 }); } catch { /* ok */ }

    // ── Discover all repos to check ──────────────────────────────────────────
    // Map<repo, { modules: [{name, version}], isModular: bool, localKitVersion: string }>
    const repoMap = new Map();

    // Source 1: installedModules (v3) — per-module tracking
    for (const [name, entry] of Object.entries(metadata.installedModules || {})) {
      if (!entry.repository || cwdRemotes.includes(entry.repository)) continue;
      if (!repoMap.has(entry.repository)) repoMap.set(entry.repository, { modules: [], isModular: true });
      repoMap.get(entry.repository).modules.push({ name, version: (entry.version || '0.0.0').replace(/^v/, '') });
    }

    // Source 2: t1k-config-*.json — each config declares its repo (catches core + any kit not in installedModules)
    for (const cf of fs.readdirSync(claudeDir).filter(f => f.startsWith('t1k-config-') && f.endsWith('.json'))) {
      try {
        const config = JSON.parse(fs.readFileSync(path.join(claudeDir, cf), 'utf8'));
        const repo = config.repos?.primary;
        if (!repo || cwdRemotes.includes(repo) || repoMap.has(repo)) continue;
        // Kit-level entry (not per-module) — use metadata.version as local version
        const localVersion = (metadata.version || '0.0.0').replace(/^v/, '');
        repoMap.set(repo, { modules: [], isModular: false, localKitVersion: localVersion });
      } catch { /* skip */ }
    }

    if (repoMap.size === 0) { fs.writeFileSync(cacheFile, new Date().toISOString()); process.exit(0); }

    // ── Check each repo for updates ──────────────────────────────────────────
    for (const [repo, info] of repoMap) {
      try {
        if (info.isModular && info.modules.length > 0) {
          checkModularRepo(repo, info.modules, metadata, metadataPath, claudeDir, cwd, fs, path, execSync);
        } else {
          checkKitRepo(repo, info.localKitVersion, metadata, metadataPath, claudeDir, cwd, fs, path, execSync);
        }
      } catch { /* skip repo, retry next session */ }
    }

    // ── Auto-commit updated .claude/ files ──────────────────────────────────
    autoCommitUpdates(cwd, execSync);

    fs.writeFileSync(cacheFile, new Date().toISOString());
    process.exit(0);
  } catch { process.exit(0); } // fail-open
})();

// ── Auto-commit helper ──────────────────────────────────────────────────────

/**
 * Auto-commit .claude/ changes after kit/module updates.
 * Only stages .claude/ files — never touches user's working changes.
 * Skips if no .claude/ changes detected or if git state is unsafe (rebase, merge).
 */
function autoCommitUpdates(cwd, execSync) {
  try {
    // Check if we're in a git repo and not in a conflicted state
    const gitStatus = execSync('git status --porcelain 2>/dev/null', { encoding: 'utf8', cwd, timeout: 5000 });
    if (!gitStatus.trim()) return; // nothing changed

    // Abort if mid-rebase or mid-merge
    const fs = require('fs');
    const path = require('path');
    const gitDir = path.join(cwd, '.git');
    if (fs.existsSync(path.join(gitDir, 'MERGE_HEAD')) || fs.existsSync(path.join(gitDir, 'rebase-merge')) || fs.existsSync(path.join(gitDir, 'rebase-apply'))) return;

    // Collect only .claude/ changes (new, modified, deleted)
    const claudeChanges = gitStatus.split('\n')
      .map(l => l.trim())
      .filter(l => l.length > 0)
      .filter(l => {
        // Extract file path — status is first 2 chars, then space, then path
        const filePath = l.substring(3).trim().replace(/^"(.*)"$/, '$1');
        return filePath.startsWith('.claude/');
      })
      .map(l => l.substring(3).trim().replace(/^"(.*)"$/, '$1'));

    if (claudeChanges.length === 0) return; // no .claude/ changes

    // Stage only .claude/ files
    execSync('git add .claude/', { cwd, timeout: 5000 });

    // Build commit message with updated module/kit names
    // Parse [t1k:updated] lines from earlier output isn't possible here,
    // so detect what changed from git diff --cached
    let diffSummary = '';
    try {
      diffSummary = execSync('git diff --cached --name-only 2>/dev/null', { encoding: 'utf8', cwd, timeout: 5000 }).trim();
    } catch { /* ok */ }

    if (!diffSummary) return; // nothing staged after add

    const changedFiles = diffSummary.split('\n').filter(Boolean);
    const updatedNames = new Set();
    for (const f of changedFiles) {
      // Detect module from path: .claude/modules/{name}/
      const moduleMatch = f.match(/\.claude\/modules\/([^/]+)\//);
      if (moduleMatch) { updatedNames.add(moduleMatch[1]); continue; }
      // Detect skill from path: .claude/skills/{name}/
      const skillMatch = f.match(/\.claude\/skills\/([^/]+)\//);
      if (skillMatch) { updatedNames.add(skillMatch[1]); continue; }
      // Detect agent from path: .claude/agents/{name}.md
      const agentMatch = f.match(/\.claude\/agents\/([^/]+)\.md$/);
      if (agentMatch) { updatedNames.add(agentMatch[1]); continue; }
      // Detect rule from path: .claude/rules/{name}.md
      const ruleMatch = f.match(/\.claude\/rules\/([^/]+)\.md$/);
      if (ruleMatch) { updatedNames.add(ruleMatch[1]); continue; }
      // Registry/config files
      if (f.match(/\.claude\/t1k-/)) { updatedNames.add('registry'); continue; }
      if (f === '.claude/metadata.json') updatedNames.add('metadata');
    }

    // Build a descriptive scope: list up to 5 names, then "and N more"
    const names = [...updatedNames];
    let scope;
    if (names.length === 0) scope = 'kit';
    else if (names.length <= 5) scope = names.join(', ');
    else scope = `${names.slice(0, 5).join(', ')} +${names.length - 5} more`;

    const msg = `chore(deps): auto-update ${scope}\n\nAuto-committed by check-kit-updates hook.\nFiles: ${changedFiles.length} changed in .claude/`;

    execSync(`git commit -m "${msg.replace(/"/g, '\\"')}" --no-verify 2>/dev/null`, { cwd, timeout: 10000 });
    console.log(`[t1k:auto-commit] Committed ${changedFiles.length} .claude/ file(s) from kit/module update`);
  } catch {
    // fail-open: if commit fails, updates are still extracted — user can commit manually
  }
}

// ── Helpers ──────────────────────────────────────────────────────────────────

function readMetadata(metadataPath) {
  const fs = require('fs');
  try { return JSON.parse(fs.readFileSync(metadataPath, 'utf8')); } catch { return null; }
}

function writeMetadata(metadataPath, data) {
  const fs = require('fs');
  try { fs.writeFileSync(metadataPath, JSON.stringify(data, null, 2) + '\n'); } catch { /* ok */ }
}

/**
 * Check a modular repo: fetch manifest.json, compare per-module versions, download ZIPs.
 */
function checkModularRepo(repo, modules, metadata, metadataPath, claudeDir, cwd, fs, path, execSync) {
  let manifest;
  try {
    const raw = execSync(`gh release download --repo ${repo} --pattern "manifest.json" --output - 2>/dev/null`, { encoding: 'utf8', timeout: 10000 });
    const parsed = JSON.parse(raw);
    manifest = parsed.modules || parsed;
  } catch {
    // Fallback: use release tag for all modules
    try {
      const tag = JSON.parse(execSync(`gh release view --repo ${repo} --json tagName 2>/dev/null`, { encoding: 'utf8', timeout: 10000 })).tagName.replace(/^v/, '');
      manifest = Object.fromEntries(modules.map(m => [m.name, { version: tag }]));
    } catch { return; }
  }

  for (const { name, version: local } of modules) {
    const remote = (manifest[name]?.version || '').replace(/^v/, '');
    if (!remote || remote === local) continue;

    if (isMajorBump(local, remote)) {
      console.log(`[t1k:major-update] ${name} ${local} → ${remote} (major). Run: gh release download --repo ${repo} --pattern "${name}-*.zip"`);
    } else {
      try {
        // Save old manifest file list before overwriting
        const oldManifestPath = path.join(claudeDir, 'modules', name, '.t1k-manifest.json');
        let oldFiles = [];
        try { oldFiles = JSON.parse(fs.readFileSync(oldManifestPath, 'utf8')).files || []; } catch { /* no old manifest */ }

        const tmpZip = path.join(claudeDir, `.${name}-update.zip`);
        execSync(`gh release download --repo ${repo} --pattern "${name}-*.zip" --output "${tmpZip}" --clobber 2>/dev/null`, { timeout: 30000 });
        extractZip(tmpZip, cwd);
        try { fs.unlinkSync(tmpZip); } catch { /* ok */ }

        // Clean up orphaned files (removed between versions)
        let newFiles = [];
        try { newFiles = JSON.parse(fs.readFileSync(oldManifestPath, 'utf8')).files || []; } catch { /* ok */ }
        const newSet = new Set(newFiles);
        for (const f of oldFiles) {
          if (!newSet.has(f)) {
            const fullPath = path.join(claudeDir, f);
            try { fs.rmSync(fullPath, { recursive: true, force: true }); } catch { /* ok */ }
          }
        }

        const m = readMetadata(metadataPath);
        if (m?.installedModules?.[name]) { m.installedModules[name].version = remote; writeMetadata(metadataPath, m); }
        console.log(`[t1k:updated] ${name} ${local} → ${remote}`);
      } catch { /* retry next session */ }
    }
  }
}

/**
 * Check a kit-level repo (non-modular, e.g., core): fetch release tag, compare, download ZIP.
 */
function checkKitRepo(repo, localVersion, metadata, metadataPath, claudeDir, cwd, fs, path, execSync) {
  if (!localVersion || localVersion === '0.0.0-source' || localVersion === '0.0.0') return;

  const rel = JSON.parse(execSync(`gh release view --repo ${repo} --json tagName,assets 2>/dev/null`, { encoding: 'utf8', timeout: 10000 }));
  const remote = rel.tagName.replace(/^v/, '');
  if (remote === localVersion) return;

  const kitName = repo.split('/').pop();
  if (isMajorBump(localVersion, remote)) {
    console.log(`[t1k:major-update] ${kitName} ${localVersion} → ${remote} (major). Run: gh release download --repo ${repo} --pattern "*.zip"`);
  } else if (rel.assets?.find(a => a.name.endsWith('.zip'))) {
    const tmpZip = path.join(claudeDir, `.${kitName}-update.zip`);
    execSync(`gh release download --repo ${repo} --pattern "*.zip" --output "${tmpZip}" --clobber 2>/dev/null`, { timeout: 30000 });
    extractZip(tmpZip, cwd);
    try { fs.unlinkSync(tmpZip); } catch { /* ok */ }
    const m = readMetadata(metadataPath);
    if (m) { m.version = remote; writeMetadata(metadataPath, m); }
    console.log(`[t1k:updated] ${kitName} ${localVersion} → ${remote}`);
  }
}
