#!/usr/bin/env node
// t1k-origin: kit=theonekit-core | repo=The1Studio/theonekit-core | module=null | protected=true
// notify-discord.cjs — Stop hook: sends Discord webhook notification on session end
// Reads DISCORD_WEBHOOK_URL env var. Silent no-op if not configured. Fail-open.
'use strict';
try {
  const webhookUrl = process.env.DISCORD_WEBHOOK_URL || process.env.T1K_DISCORD_WEBHOOK;
  if (!webhookUrl) process.exit(0); // not configured — silent skip

  const input = JSON.parse(process.argv[2] || '{}');
  const stopReason = input.stop_reason || 'session_end';

  // Only notify on meaningful stop events
  const notifyEvents = (process.env.T1K_NOTIFY_EVENTS || 'session-end').split(',').map(s => s.trim());
  const eventMap = { end_turn: 'session-end', stop_sequence: 'session-end', tool_use: null };
  const mappedEvent = eventMap[stopReason] || 'session-end';
  if (!notifyEvents.includes(mappedEvent)) process.exit(0);

  const https = require('https');
  const url = require('url');

  const projectName = process.env.T1K_PROJECT_NAME
    || (process.cwd().split('/').pop())
    || 'unknown-project';

  const payload = JSON.stringify({
    username: 'TheOneKit',
    embeds: [{
      title: `Session ended — ${projectName}`,
      description: `Stop reason: \`${stopReason}\``,
      color: 0x5865F2,
      timestamp: new Date().toISOString(),
      footer: { text: 'theonekit-core notify-discord' },
    }],
  });

  const parsed = url.parse(webhookUrl);
  const options = {
    hostname: parsed.hostname,
    path: parsed.path,
    method: 'POST',
    headers: { 'Content-Type': 'application/json', 'Content-Length': Buffer.byteLength(payload) },
  };

  const req = https.request(options);
  req.on('error', () => {}); // fail-open — never block on network error
  req.setTimeout(3000, () => req.destroy());
  req.write(payload);
  req.end();

  process.exit(0);
} catch (e) {
  process.exit(0); // fail-open
}
