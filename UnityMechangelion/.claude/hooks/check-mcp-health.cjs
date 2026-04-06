#!/usr/bin/env node
// t1k-origin: kit=theonekit-core | repo=The1Studio/theonekit-core | module=null | protected=true
// check-mcp-health.cjs — SessionStart hook: validate required/recommended MCP servers
// Reads t1k-config-*.json → mcp section, checks against `claude mcp list` output.
'use strict';
try {
  const fs = require('fs');
  const path = require('path');
  const { execSync } = require('child_process');
  const cwd = process.cwd();
  const claudeDir = path.join(cwd, '.claude');
  if (!fs.existsSync(claudeDir)) process.exit(0);

  // ── Collect MCP requirements from all config fragments ──
  const required = [];
  const recommended = [];
  try {
    for (const f of fs.readdirSync(claudeDir).filter(f => f.startsWith('t1k-config-') && f.endsWith('.json'))) {
      try {
        const config = JSON.parse(fs.readFileSync(path.join(claudeDir, f), 'utf8'));
        if (!config.mcp) continue;
        if (Array.isArray(config.mcp.required)) {
          for (const entry of config.mcp.required) required.push(entry);
        }
        if (Array.isArray(config.mcp.recommended)) {
          for (const entry of config.mcp.recommended) recommended.push(entry);
        }
      } catch { /* skip malformed */ }
    }
  } catch { /* ok */ }

  if (required.length === 0 && recommended.length === 0) process.exit(0);

  // ── Deduplicate by name ──
  const dedup = (arr) => {
    const seen = new Set();
    return arr.filter(e => { if (seen.has(e.name)) return false; seen.add(e.name); return true; });
  };
  const reqList = dedup(required);
  const recList = dedup(recommended);

  // ── Get connected MCP servers ──
  let connectedServers = new Set();
  try {
    const output = execSync('claude mcp list 2>/dev/null', {
      encoding: 'utf8',
      timeout: 10000,
      stdio: ['pipe', 'pipe', 'pipe'],
    });
    // Parse output lines: "name: command/url - status"
    for (const line of output.split('\n')) {
      const trimmed = line.trim();
      if (!trimmed) continue;
      // Match pattern: "server-name: ..." or just extract first word/token before ":"
      const colonIdx = trimmed.indexOf(':');
      if (colonIdx > 0) {
        const name = trimmed.substring(0, colonIdx).trim();
        // Check if connected (contains "Connected" or does NOT contain "Needs authentication" or "Error")
        const isConnected = trimmed.includes('Connected') || (!trimmed.includes('Needs authentication') && !trimmed.includes('Error'));
        if (isConnected) connectedServers.add(name.toLowerCase());
      }
    }
  } catch {
    // If claude mcp list fails, skip MCP health check silently
    process.exit(0);
  }

  // ── Also check global MCP config files as fallback ──
  const mcpConfigPaths = [
    path.join(process.env.HOME || '', '.claude', 'mcp.json'),
    path.join(cwd, '.mcp.json'),
  ];
  for (const mcpPath of mcpConfigPaths) {
    try {
      const mcpConfig = JSON.parse(fs.readFileSync(mcpPath, 'utf8'));
      if (mcpConfig.mcpServers) {
        for (const name of Object.keys(mcpConfig.mcpServers)) {
          connectedServers.add(name.toLowerCase());
        }
      }
    } catch { /* ok */ }
  }

  // ── Check missing ──
  const lines = [];
  const missingRequired = reqList.filter(e => !connectedServers.has(e.name.toLowerCase()));
  const missingRecommended = recList.filter(e => !connectedServers.has(e.name.toLowerCase()));

  for (const entry of missingRequired) {
    lines.push(`[t1k:mcp-missing] REQUIRED: ${entry.name} — ${entry.purpose}. Install: ${entry.installCmd}`);
  }
  for (const entry of missingRecommended) {
    lines.push(`[t1k:mcp-suggest] RECOMMENDED: ${entry.name} — ${entry.purpose}. Install: ${entry.installCmd}`);
  }

  if (lines.length > 0) {
    console.log(lines.join('\n'));
  }

  process.exit(0);
} catch (e) {
  // fail-open: never block session start
  process.exit(0);
}
