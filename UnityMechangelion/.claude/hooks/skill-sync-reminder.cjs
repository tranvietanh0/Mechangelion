#!/usr/bin/env node
// t1k-origin: kit=theonekit-core | repo=The1Studio/theonekit-core | module=null | protected=true
/**
 * skill-sync-reminder.cjs - Remind AI to run sync-back after skill edits
 *
 * PostToolUse hook for Edit/Write tools.
 * When a file under .claude/skills/ is modified, outputs a reminder
 * to run /t1k:sync-back --dry-run before committing.
 *
 * Non-blocking (PostToolUse cannot block). Just a nudge.
 * Standalone — no shared lib dependencies. Ships with theonekit-core.
 */
(async () => {
  try {
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

    // Only check Edit and Write
    if (!['Edit', 'Write'].includes(toolName)) {
      process.exit(0);
    }

    const filePath = toolInput?.file_path || '';
    if (!filePath) {
      process.exit(0);
    }

    // Check if file is under .claude/skills/ (exact segment match)
    if (!/[/\\]\.claude[/\\]skills[/\\]/.test(filePath)) {
      process.exit(0);
    }

    // Output reminder (non-blocking — PostToolUse just informs)
    console.log(`[t1k:skill-modified] file="${filePath}" → Run /t1k:sync-back --dry-run before commit`);

    process.exit(0);
  } catch (e) {
    // Fail-open
    process.exit(0);
  }
})();
