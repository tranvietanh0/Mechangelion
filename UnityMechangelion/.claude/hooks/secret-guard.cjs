#!/usr/bin/env node
// t1k-origin: kit=theonekit-core | repo=The1Studio/theonekit-core | module=null | protected=true
/**
 * secret-guard.cjs - Block git operations that would commit sensitive files
 *
 * PreToolUse hook for Bash tool.
 * Hard-blocks:
 *   - git add of sensitive files (.env*, *.pem, *.key, credentials*)
 *   - git add -A / git add . when sensitive files exist in working tree
 *   - git commit when sensitive files are staged
 *   - git push when sensitive files are staged (final gate)
 *
 * No approval flow — must unstage manually. This is a hard security gate.
 * Standalone — no shared lib dependencies. Ships with theonekit-core.
 */
(async () => {
  try {
    const { execSync } = require('child_process');
    const path = require('path');

    // Sensitive file patterns for basename matching
    const SENSITIVE_PATTERNS = [
      /^\.env$/,
      /^\.env\./,
      /credentials/i,
      /secrets?\.ya?ml$/i,
      /\.pem$/,
      /\.key$/,
      /id_rsa/,
      /id_ed25519/,
      /\.p12$/,
      /\.pfx$/,
      /\.jks$/,
      /serviceaccount.*\.json$/i,
    ];

    // Safe patterns — exempt (templates, examples)
    const SAFE_PATTERNS = [
      /\.example$/i,
      /\.sample$/i,
      /\.template$/i,
    ];

    function isSensitiveFile(filePath) {
      if (!filePath) return false;
      const base = path.basename(filePath);
      if (SAFE_PATTERNS.some(p => p.test(base))) return false;
      return SENSITIVE_PATTERNS.some(p => p.test(base));
    }

    function getStagedFiles() {
      try {
        const output = execSync('git diff --cached --name-only 2>/dev/null', {
          encoding: 'utf8',
          timeout: 5000,
        });
        return output.trim().split('\n').filter(Boolean);
      } catch {
        return [];
      }
    }

    function getUntrackedSensitiveFiles() {
      try {
        const output = execSync('git ls-files --others --exclude-standard 2>/dev/null', {
          encoding: 'utf8',
          timeout: 5000,
        });
        return output.trim().split('\n').filter(Boolean).filter(isSensitiveFile);
      } catch {
        return [];
      }
    }

    function getModifiedSensitiveFiles() {
      try {
        const output = execSync('git diff --name-only 2>/dev/null', {
          encoding: 'utf8',
          timeout: 5000,
        });
        return output.trim().split('\n').filter(Boolean).filter(isSensitiveFile);
      } catch {
        return [];
      }
    }

    // Read stdin
    let input = '';
    for await (const chunk of process.stdin) {
      input += chunk;
    }

    let hookData;
    try {
      hookData = JSON.parse(input);
    } catch {
      process.exit(0);
    }

    const { tool_name: toolName, tool_input: toolInput } = hookData;

    // Only check Bash commands
    if (toolName !== 'Bash' || !toolInput?.command) {
      process.exit(0);
    }

    const cmd = toolInput.command.trim();

    // === Check 1: git add of sensitive files directly ===
    const gitAddMatch = cmd.match(/git\s+add\s+(.+)/);
    if (gitAddMatch) {
      const args = gitAddMatch[1].trim();

      // git add -A or git add . — check for sensitive files in working tree
      if (args === '-A' || args === '.' || args === '--all') {
        const sensitiveUntracked = getUntrackedSensitiveFiles();
        const sensitiveModified = getModifiedSensitiveFiles();
        const allSensitive = [...new Set([...sensitiveUntracked, ...sensitiveModified])];

        if (allSensitive.length > 0) {
          console.error(`
\x1b[31mSECURITY BLOCK\x1b[0m: "${cmd}" would stage sensitive files:

${allSensitive.map(f => `  \x1b[31m✗\x1b[0m ${f}`).join('\n')}

  \x1b[34mFix:\x1b[0m Stage specific files instead of using "${args}":
    git add file1.ts file2.ts

  \x1b[90mOr add these to .gitignore first.\x1b[0m
`);
          process.exit(2); // Hard block
        }
      }

      // git add specific-file — check each file
      const files = args.split(/\s+/).filter(f => !f.startsWith('-'));
      const sensitiveFiles = files.filter(isSensitiveFile);
      if (sensitiveFiles.length > 0) {
        console.error(`
\x1b[31mSECURITY BLOCK\x1b[0m: Cannot stage sensitive files:

${sensitiveFiles.map(f => `  \x1b[31m✗\x1b[0m ${f}`).join('\n')}

  These files may contain secrets and must not be committed.
  \x1b[34mFix:\x1b[0m Add them to .gitignore or use .env.example for templates.
`);
        process.exit(2); // Hard block
      }
    }

    // === Check 2: git commit — verify no sensitive files are staged ===
    if (/git\s+commit/.test(cmd)) {
      const staged = getStagedFiles();
      const sensitiveStaged = staged.filter(isSensitiveFile);

      if (sensitiveStaged.length > 0) {
        console.error(`
\x1b[31mSECURITY BLOCK\x1b[0m: Sensitive files are staged for commit:

${sensitiveStaged.map(f => `  \x1b[31m✗\x1b[0m ${f}`).join('\n')}

  \x1b[34mFix:\x1b[0m Unstage them first:
${sensitiveStaged.map(f => `    git reset HEAD "${f}"`).join('\n')}
`);
        process.exit(2); // Hard block
      }
    }

    // === Check 3: git push — final gate, check staged files ===
    if (/git\s+push/.test(cmd)) {
      const staged = getStagedFiles();
      const sensitiveStaged = staged.filter(isSensitiveFile);

      if (sensitiveStaged.length > 0) {
        console.error(`
\x1b[31mSECURITY BLOCK\x1b[0m: Cannot push — sensitive files are staged:

${sensitiveStaged.map(f => `  \x1b[31m✗\x1b[0m ${f}`).join('\n')}

  \x1b[34mFix:\x1b[0m Unstage and reset before pushing.
`);
        process.exit(2); // Hard block
      }
    }

    process.exit(0); // Allow
  } catch (e) {
    // Fail-open: if hook crashes, allow the action
    process.exit(0);
  }
})();
