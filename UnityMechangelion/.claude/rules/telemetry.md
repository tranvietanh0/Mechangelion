---

origin: theonekit-core
repository: The1Studio/theonekit-core
module: null
protected: true
---
# Telemetry System

## Overview

TheOneKit collects anonymous usage telemetry to improve skills and identify gaps.
Enabled by default (`features.telemetry: true`). Opt out via any `t1k-config-*.json`:

```json
{ "features": { "telemetry": false } }
```

## Data Collected (via hooks)

| Type | Hook | File | What |
|------|------|------|------|
| Errors | PostToolUse on Bash (non-zero) | `errors-{date}.jsonl` | Command, error head (200 chars), timestamp |
| Skill usage | PostToolUse on Skill | `usage-{date}.jsonl` | Skill name, args (truncated), timestamp |
| Feature gaps | AI-driven (this rule) | `gaps-{date}.jsonl` | Query, matched skills, suggestion |

## Feature Gap Detection (AI-Driven)

When processing a user request and NO matching skill exists:

1. Check activation fragments — if zero skills match the topic
2. Log to `.claude/telemetry/gaps-{date}.jsonl`:
   ```json
   {"ts":"...","query":"ECS batch processing","matchedSkills":[],"suggestion":"Need ECS batch skill"}
   ```
3. Continue with the task — gap logging is passive, never blocks work

## Privacy Safeguards

- **No source code** — only command names, error types, skill names
- **No absolute file paths** — only relative project paths
- **stderr truncated** to 200 chars max
- **No secrets** — commands are logged but arguments containing env vars are stripped
- **Local first** — all data stays in `.claude/telemetry/` (gitignored)
- **User reviews** every batch before GitHub submission

## Error Threshold Auto-Trigger

When 3+ errors are logged in a session, the error collector outputs:
```
[t1k:telemetry-threshold] 3 errors logged (threshold: 3). Run /t1k:watzup now...
```

The AI reads this and should run `/t1k:watzup` to review error patterns.
- Threshold fires once per session (debounced via `.threshold-triggered` marker)
- Stop hook provides fallback reminder if threshold was reached but watzup wasn't run
- Markers (`.threshold-triggered`, `.reminded`) are cleaned up when telemetry is archived

## GitHub Submission Protocol (at session wrap)

During `/t1k:watzup`, the telemetry section:

1. **Read** all `.claude/telemetry/*.jsonl` files from current session date
2. **Aggregate**:
   - Error patterns: group by error type, count occurrences
   - Skill usage: rank by frequency, identify never-used skills
   - Feature gaps: list unique queries with no matching skills
3. **Present summary** to user in watzup output
4. **Offer submission**: "Submit telemetry to kit repo as GitHub issue?"
5. **If approved**: create issue via `gh issue create` on the kit repo from `t1k-config-*.json → repos.primary`
   - Title: `[telemetry] Session report {date}`
   - Label: `telemetry`
   - Body: aggregated stats (no raw data)
6. **After submission**: archive processed files to `.claude/telemetry/archived/`

## Issue Format

```markdown
## Telemetry Report — {date}

### Error Patterns (top 5)
| Error | Count | Example Command |
|-------|-------|-----------------|
| TypeError: Cannot read... | 3 | npm test |

### Skill Usage
| Skill | Activations |
|-------|------------|
| t1k-cook | 5 |
| t1k-fix | 3 |

### Feature Gaps
| Query | Suggestion |
|-------|-----------|
| ECS batch processing | Need ECS batch skill |

### Never-Used Skills (this session)
- skill-a, skill-b
```

## File Lifecycle

```
Hook fires → .claude/telemetry/{type}-{date}.jsonl (append)
  → /t1k:watzup reads + aggregates
  → User approves → gh issue create
  → Processed files → .claude/telemetry/archived/{type}-{date}.jsonl
```

## Cloud Prompt Telemetry

In addition to local JSONL telemetry, T1K collects prompt data to a Cloudflare Worker + D1.

### Architecture
```
UserPromptSubmit hook (prompt-telemetry.cjs)
  → Sanitize prompt (strip secrets, paths, truncate 2000 chars)
  → POST to CF Worker with GitHub token auth
  → Worker verifies org membership → inserts to D1
  → Piggyback: previous prompt's outcome sent with current prompt

Stop hook (prompt-telemetry-flush.cjs)
  → Sends last prompt's outcome on session end
  → Cleans up session cache files
```

### Data Collected (Cloud)
| Field | Source |
|-------|--------|
| prompt (sanitized) | UserPromptSubmit stdin |
| user | GitHub API (server-verified) |
| project, kit, modules | .claude/metadata.json |
| classifiedAs | Hook keyword matching |
| outcome, errorCount, duration | Piggyback from next prompt or Stop flush |

### Auth
- GitHub token from `gh auth token` (cached 30 min locally)
- Worker verifies via GitHub API → The1Studio org membership
- Token hash cached in CF KV (5-min TTL), raw token discarded

### Sanitization
Prompts are stripped of: API key patterns (sk-*, ghp_*, AKIA*), passwords, env var values, home paths.

### Config
Endpoint hardcoded in `t1k-config-core.json` → `telemetry.cloud.endpoint`.
Override via `T1K_TELEMETRY_ENDPOINT` env var.
Disable cloud telemetry: set `telemetry.cloud.enabled: false` in config.

### Query
```bash
# Via wrangler CLI
npx wrangler d1 execute t1k-telemetry --remote --command="SELECT * FROM prompts LIMIT 10"
```
