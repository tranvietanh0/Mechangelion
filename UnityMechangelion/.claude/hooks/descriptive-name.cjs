#!/usr/bin/env node
// t1k-origin: kit=theonekit-core | repo=The1Studio/theonekit-core | module=null | protected=true
// descriptive-name.cjs — PreToolUse hook on Write: warns if new files don't follow naming conventions
// Warn only (exit 2) — does not block writes. Fail-open on errors.
'use strict';
try {
  const input = JSON.parse(process.argv[2] || '{}');
  const filePath = (input.tool_input || {}).file_path || '';
  if (!filePath) process.exit(0);

  const path = require('path');
  const basename = path.basename(filePath);
  const ext = path.extname(basename).toLowerCase();
  const name = basename.slice(0, -ext.length); // filename without extension

  // Extensions to skip (no convention enforced)
  const SKIP_EXTS = new Set(['.md', '.json', '.yml', '.yaml', '.txt', '.env',
    '.gitignore', '.gitattributes', '.editorconfig', '.prettierrc', '.eslintrc',
    '.babelrc', '.nvmrc', '', '.lock', '.log', '.xml', '.csv', '.toml']);
  if (SKIP_EXTS.has(ext)) process.exit(0);

  // Extensions that require kebab-case
  const KEBAB_EXTS = new Set(['.js', '.ts', '.jsx', '.tsx', '.cjs', '.mjs',
    '.py', '.sh', '.bash', '.zsh', '.rb', '.php']);

  // Extensions that require PascalCase
  const PASCAL_EXTS = new Set(['.cs', '.java', '.kt', '.swift', '.fs', '.vb']);

  // Extensions that require snake_case
  const SNAKE_EXTS = new Set(['.go', '.rs']);

  function isKebabCase(s) {
    return /^[a-z0-9]+(-[a-z0-9]+)*$/.test(s);
  }

  function isPascalCase(s) {
    return /^[A-Z][a-zA-Z0-9]*$/.test(s);
  }

  function isSnakeCase(s) {
    return /^[a-z0-9]+(_[a-z0-9]+)*$/.test(s);
  }

  function toKebab(s) {
    return s
      .replace(/([A-Z])/g, '-$1')
      .replace(/_/g, '-')
      .replace(/--+/g, '-')
      .toLowerCase()
      .replace(/^-/, '');
  }

  let violated = false;
  let message = '';

  if (KEBAB_EXTS.has(ext)) {
    if (!isKebabCase(name)) {
      violated = true;
      const suggestion = toKebab(name);
      message = `naming: '${basename}' should use kebab-case. Suggested: '${suggestion}${ext}'`;
    }
  } else if (PASCAL_EXTS.has(ext)) {
    if (!isPascalCase(name)) {
      violated = true;
      message = `naming: '${basename}' should use PascalCase (e.g., 'MyClass${ext}')`;
    }
  } else if (SNAKE_EXTS.has(ext)) {
    if (!isSnakeCase(name)) {
      violated = true;
      message = `naming: '${basename}' should use snake_case (e.g., 'my_module${ext}')`;
    }
  }

  if (violated) {
    // Exit 2 = warn, not block (Claude Code treats exit 2 as advisory)
    console.log(JSON.stringify({
      decision: 'warn',
      reason: `descriptive-name: ${message}`,
    }));
    process.exit(2);
  }

  process.exit(0);
} catch (e) {
  process.exit(0); // fail-open
}
