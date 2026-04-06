#!/usr/bin/env node
// t1k-origin: kit=theonekit-core | repo=The1Studio/theonekit-core | module=null | protected=true
'use strict';

/**
 * TheOneKit Hook Runner — Cross-platform hook bootstrapper
 *
 * Resolves the project root (git root or directory walk) and runs the
 * target hook script from the correct .claude/hooks/ location.
 *
 * Usage: node .claude/hooks/hook-runner.cjs <hook-name> [extra-args...]
 *
 * This solves the subdirectory CWD problem: when Claude Code's CWD changes
 * to a subdirectory (e.g., src/, docs/), relative paths to .claude/hooks/
 * break. This bootstrapper always resolves from the project root.
 *
 * The CLI's transformClaudePaths() handles this file's simple relative path,
 * converting it to $HOME (global) or $CLAUDE_PROJECT_DIR (local) during install.
 */

const { execSync } = require('child_process');
const path = require('path');
const fs = require('fs');

/**
 * Find the project root containing .claude/hooks/
 * Strategy: git rev-parse first, then walk up from __dirname
 */
function findProjectRoot() {
  // Strategy 1: git rev-parse (works in any subdirectory of a git repo)
  try {
    const gitRoot = execSync('git rev-parse --show-toplevel', {
      encoding: 'utf8',
      stdio: ['pipe', 'pipe', 'ignore'],
      timeout: 3000,
      windowsHide: true
    }).trim();
    if (gitRoot && fs.existsSync(path.join(gitRoot, '.claude', 'hooks'))) {
      return gitRoot;
    }
  } catch {}

  // Strategy 2: walk up from this file's location
  // hook-runner.cjs lives in .claude/hooks/, so project root is 2 levels up
  const fromDirname = path.resolve(__dirname, '..', '..');
  if (fs.existsSync(path.join(fromDirname, '.claude', 'hooks'))) {
    return fromDirname;
  }

  // Strategy 3: walk up from CWD
  let dir = process.cwd();
  for (let i = 0; i < 10; i++) {
    if (fs.existsSync(path.join(dir, '.claude', 'hooks'))) {
      return dir;
    }
    const parent = path.dirname(dir);
    if (parent === dir) break;
    dir = parent;
  }

  // Fallback: CWD (best effort)
  return process.cwd();
}

// Main
const hookName = process.argv[2];
if (!hookName) {
  process.stderr.write('hook-runner: missing hook name argument\n');
  process.exit(1);
}

const root = findProjectRoot();
const hookPath = path.join(root, '.claude', 'hooks', `${hookName}.cjs`);

if (!fs.existsSync(hookPath)) {
  // Silent exit — hook may not exist in all kits
  process.exit(0);
}

// Forward stdin to the hook module
// Hooks receive data on stdin (JSON from Claude Code)
// We require() the hook so it can read process.stdin directly
require(hookPath);
