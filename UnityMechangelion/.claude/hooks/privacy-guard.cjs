#!/usr/bin/env node
// t1k-origin: kit=theonekit-core | repo=The1Studio/theonekit-core | module=null | protected=true
/**
 * privacy-guard.cjs - Block reading sensitive files unless user approves
 *
 * PreToolUse hook for Read/Glob/Grep tools.
 * Blocks: .env*, *.pem, *.key, id_rsa*, credentials*, secrets.yml
 * Exempts: .env.example, .env.sample, .env.template
 *
 * Approval flow:
 * 1. Claude tries Read ".env" → BLOCKED (exit 2)
 * 2. Claude asks user via AskUserQuestion
 * 3. If approved → Claude uses bash: cat ".env"
 *
 * Standalone — no shared lib dependencies. Ships with theonekit-core.
 */
(async () => {
  try {
    const path = require('path');

    // Sensitive file patterns
    const SENSITIVE_PATTERNS = [
      // Env files
      /^\.env$/,
      /^\.env\./,
      /\/\.env$/,
      /\/\.env\./,
      /credentials/i,
      /secrets?\.ya?ml$/i,

      // SSL/TLS & Certificates
      /\.pem$/,
      /\.key$/,
      /\.crt$/,
      /\.p12$/,
      /\.pfx$/,
      /\.jks$/,
      /\.keystore$/,
      /\.truststore$/,

      // SSH keys
      /id_rsa/,
      /id_ed25519/,
      /id_ecdsa/,
      /known_hosts$/,

      // Service accounts
      /serviceaccount.*\.json$/i,

      // AWS
      /\.aws\/credentials$/,
      /\.aws\/config$/,
      /aws-exports\.js$/,

      // GCP
      /application_default_credentials\.json$/,
      /\/\.gcp\//,

      // Azure
      /\.azure\/accessTokens\.json$/,
      /\.azure\/azureProfile\.json$/,

      // Docker
      /\.docker\/config\.json$/,
      /\.dockerconfigjson$/,

      // Kubernetes
      /kubeconfig$/,
      /-secret\.ya?ml$/,

      // CI/CD
      /\.circleci\/config\.yml$/,
      /\.travis\.yml$/,

      // Databases
      /\.pgpass$/,
      /\.my\.cnf$/,
      /mongod\.conf$/,

      // Package managers
      /\.npmrc$/,
      /\.pypirc$/,
      /\.gem\/credentials$/,

      // Terraform
      /terraform\.tfstate$/,
      /terraform\.tfvars$/,

      // IDE
      /\.idea\/dataSources\.xml$/,

      // General
      /htpasswd$/,
      /\.netrc$/,
    ];

    // Safe patterns — exempt from blocking
    const SAFE_PATTERNS = [
      /\.example$/i,
      /\.sample$/i,
      /\.template$/i,
      /\.schema$/i,
      /node_modules/,
      /\.claude\//,
    ];

    function isSafe(filePath) {
      if (!filePath) return true;
      const base = path.basename(filePath);
      return SAFE_PATTERNS.some(p => p.test(filePath) || p.test(base));
    }

    function isSensitive(filePath) {
      if (!filePath) return false;
      const base = path.basename(filePath);
      return SENSITIVE_PATTERNS.some(p => p.test(filePath) || p.test(base));
    }

    function extractFilePath(toolName, toolInput) {
      if (!toolInput) return null;
      if (toolName === 'Read' && toolInput.file_path) return toolInput.file_path;
      if (toolName === 'Glob' && toolInput.pattern) return toolInput.pattern;
      if (toolName === 'Grep' && toolInput.path) return toolInput.path;
      return null;
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

    // Only check Read, Glob, Grep
    if (!['Read', 'Glob', 'Grep'].includes(toolName)) {
      process.exit(0);
    }

    const filePath = extractFilePath(toolName, toolInput);
    if (!filePath) {
      process.exit(0);
    }

    // Safe files pass through
    if (isSafe(filePath)) {
      process.exit(0);
    }

    // Check sensitivity
    if (isSensitive(filePath)) {
      const basename = path.basename(filePath);
      console.error(`
\x1b[33mSECURITY BLOCK\x1b[0m: Sensitive file detected

  \x1b[33mFile:\x1b[0m ${filePath}
  \x1b[33mTool:\x1b[0m ${toolName}

  This file may contain secrets (API keys, passwords, tokens).

  \x1b[34mTo proceed:\x1b[0m Ask the user for permission using AskUserQuestion, then use:
    \x1b[32mbash: cat "${filePath}"\x1b[0m
  \x1b[31mIf denied:\x1b[0m Continue without reading this file.
  \x1b[90mTip: Use .env.example for documenting required variables.\x1b[0m
`);
      process.exit(2); // Block
    }

    process.exit(0); // Allow
  } catch (e) {
    // Fail-open: if hook crashes, allow the action
    process.exit(0);
  }
})();
