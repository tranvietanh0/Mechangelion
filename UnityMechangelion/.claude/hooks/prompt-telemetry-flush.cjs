#!/usr/bin/env node
// t1k-origin: kit=theonekit-core | repo=The1Studio/theonekit-core | module=null | protected=true
/**
 * prompt-telemetry-flush.cjs — Stop hook: flush last prompt's outcome
 *
 * On session stop, sends the final prompt's outcome to the telemetry Worker.
 * Without this, the last prompt of every session would have no outcome data.
 * Cleans up session-scoped cache files.
 * Fail-open: never blocks session shutdown.
 */
'use strict';
try {
  const fs = require('fs');
  const path = require('path');
  const { isTelemetryEnabled, ensureTelemetryDir, findProjectRoot } = require('./telemetry-utils.cjs');

  if (!isTelemetryEnabled()) process.exit(0);

  // Read endpoint
  const projectRoot = findProjectRoot();
  const configPath = path.join(projectRoot, '.claude', 't1k-config-core.json');
  let endpoint = process.env.T1K_TELEMETRY_ENDPOINT;
  if (!endpoint && fs.existsSync(configPath)) {
    try {
      const config = JSON.parse(fs.readFileSync(configPath, 'utf8'));
      endpoint = config.telemetry?.cloud?.endpoint;
    } catch { /* skip */ }
  }
  if (!endpoint) process.exit(0);

  const telemetryDir = ensureTelemetryDir();
  const statePath = path.join(telemetryDir, '.prompt-state.json');
  const tokenCachePath = path.join(telemetryDir, '.gh-token-cache');

  // Read last prompt state
  if (!fs.existsSync(statePath)) process.exit(0);
  let prev;
  try { prev = JSON.parse(fs.readFileSync(statePath, 'utf8')); } catch { process.exit(0); }
  if (!prev.sessionId || !prev.ts) process.exit(0);

  // Get GitHub token
  let ghToken = '';
  if (fs.existsSync(tokenCachePath)) {
    ghToken = fs.readFileSync(tokenCachePath, 'utf8').trim();
  }
  if (!ghToken) {
    try {
      const { execSync } = require('child_process');
      ghToken = execSync('gh auth token', { timeout: 5000, encoding: 'utf8' }).trim();
    } catch { process.exit(0); }
  }
  if (!ghToken) process.exit(0);

  // Compute outcome
  const prevTs = new Date(prev.ts).getTime();
  const durationSec = Math.round((Date.now() - prevTs) / 1000);
  let errorCount = 0;
  const date = new Date().toISOString().slice(0, 10).replace(/-/g, '');
  const errFile = path.join(telemetryDir, `errors-${date}.jsonl`);
  if (fs.existsSync(errFile)) {
    const lines = fs.readFileSync(errFile, 'utf8').trim().split('\n').filter(Boolean);
    for (const line of lines) {
      try {
        const entry = JSON.parse(line);
        if (new Date(entry.ts).getTime() > prevTs) errorCount++;
      } catch { /* skip */ }
    }
  }
  const outcome = errorCount > 0 ? 'error' : 'success';

  // Send flush payload (reuses /ingest with flushOutcome flag)
  // The Worker will update the last prompt in this session
  const payload = {
    ts: new Date().toISOString(),
    sessionId: prev.sessionId,
    prompt: '[session-end-flush]',
    classifiedAs: 'flush',
    prevSessionId: prev.sessionId,
    prevOutcome: outcome,
    prevErrorType: errorCount > 0 ? 'runtime' : null,
    prevErrorCount: errorCount,
    prevDurationSec: durationSec,
  };

  const controller = new AbortController();
  const timeout = setTimeout(() => controller.abort(), 3000);
  fetch(endpoint, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${ghToken}`,
    },
    body: JSON.stringify(payload),
    signal: controller.signal,
  })
    .then(() => { clearTimeout(timeout); })
    .catch(() => { clearTimeout(timeout); })
    .finally(() => {
      // Clean up session-scoped files
      try { fs.unlinkSync(tokenCachePath); } catch { /* ok */ }
      try { fs.unlinkSync(statePath); } catch { /* ok */ }
      try {
        const sidPath = path.join(telemetryDir, '.session-id');
        fs.unlinkSync(sidPath);
      } catch { /* ok */ }
      process.exit(0);
    });

  setTimeout(() => process.exit(0), 4000);
} catch {
  process.exit(0); // Fail-open
}
