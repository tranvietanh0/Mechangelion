#!/usr/bin/env node
// t1k-origin: kit=theonekit-core | repo=The1Studio/theonekit-core | module=null | protected=true
/**
 * telemetry-skill-tracker.cjs - Track skill activations
 *
 * PostToolUse hook for Skill tool.
 * Logs which skills are activated, how often, for usage analytics.
 *
 * Respects features.telemetry config flag (opt-out).
 * Standalone — no shared lib dependencies. Ships with theonekit-core.
 */
(async () => {
  try {
    const fs = require('fs');
    const path = require('path');
    const { isTelemetryEnabled, ensureTelemetryDir } = require('./telemetry-utils.cjs');

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

    if (toolName !== 'Skill') {
      process.exit(0);
    }

    if (!isTelemetryEnabled()) {
      process.exit(0);
    }

    const entry = {
      ts: new Date().toISOString(),
      skill: toolInput?.skill || 'unknown',
      args: (toolInput?.args || '').substring(0, 200),
    };

    const telemetryDir = ensureTelemetryDir();

    // Write to date-stamped JSONL file
    const date = new Date().toISOString().slice(0, 10).replace(/-/g, '');
    const filePath = path.join(telemetryDir, `usage-${date}.jsonl`);
    fs.appendFileSync(filePath, JSON.stringify(entry) + '\n');

    process.exit(0);
  } catch (e) {
    // Fail-open
    process.exit(0);
  }
})();
