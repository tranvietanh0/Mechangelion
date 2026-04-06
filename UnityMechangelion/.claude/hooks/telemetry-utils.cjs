// t1k-origin: kit=theonekit-core | repo=The1Studio/theonekit-core | module=null | protected=true
/**
 * telemetry-utils.cjs - Shared utilities for telemetry hooks
 *
 * DRY: centralizes telemetry opt-out check used by all telemetry hooks.
 * Standalone — no external dependencies. Ships with theonekit-core.
 */
const fs = require('fs');
const path = require('path');

/**
 * Check if telemetry is disabled via t1k-config-core.json.
 * Returns true if telemetry is enabled (or config unreadable — fail-open).
 */
function isTelemetryEnabled() {
  const configPath = path.join(findProjectRoot(), '.claude', 't1k-config-core.json');
  if (!fs.existsSync(configPath)) return true; // No config = enabled (fail-open)
  try {
    const config = JSON.parse(fs.readFileSync(configPath, 'utf8'));
    return !(config.features && config.features.telemetry === false);
  } catch {
    return true; // Config unreadable = enabled (fail-open)
  }
}

/**
 * Find the project root by walking up from CWD looking for .claude/ directory.
 * Falls back to CWD if no .claude/ found.
 */
function findProjectRoot() {
  // Walk up from CWD to find nearest .claude/ directory
  let dir = process.cwd();
  const root = path.parse(dir).root;
  while (dir !== root) {
    if (fs.existsSync(path.join(dir, '.claude', 'metadata.json')) ||
        fs.existsSync(path.join(dir, '.claude', 'settings.json'))) {
      return dir;
    }
    dir = path.dirname(dir);
  }
  // Fallback: check $HOME/.claude/ (global install)
  const home = process.env.HOME || process.env.USERPROFILE || '';
  if (home && fs.existsSync(path.join(home, '.claude', 'metadata.json'))) {
    return home;
  }
  return process.cwd();
}

/**
 * Ensure .claude/telemetry/ directory exists. Returns the path.
 * Uses findProjectRoot() instead of CWD for correct resolution.
 */
function ensureTelemetryDir() {
  const projectRoot = findProjectRoot();
  const dir = path.join(projectRoot, '.claude', 'telemetry');
  if (!fs.existsSync(dir)) {
    fs.mkdirSync(dir, { recursive: true });
  }
  return dir;
}

/**
 * Get or generate a session ID. Persists in .claude/telemetry/.session-id.
 */
function getSessionId() {
  const dir = ensureTelemetryDir();
  const sidPath = path.join(dir, '.session-id');
  if (fs.existsSync(sidPath)) {
    return fs.readFileSync(sidPath, 'utf8').trim();
  }
  const sid = require('crypto').randomUUID();
  fs.writeFileSync(sidPath, sid);
  return sid;
}

module.exports = { isTelemetryEnabled, ensureTelemetryDir, getSessionId, findProjectRoot };
