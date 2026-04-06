---

origin: theonekit-core
repository: The1Studio/theonekit-core
module: null
protected: true
---
# TheOneKit Error Recovery

## Registry-Based Error Routing

**MANDATORY:** Read ALL `t1k-routing-*.json` files.
Look up the `errorRecovery` map — match error type to role, then resolve role to agent.

| Error Type | Recovery Role | Command |
|------------|--------------|---------|
| Compilation error | `implementer` | `/t1k:fix` |
| Test failure | `tester` | `/t1k:fix --test` |
| Runtime error | `debugger` | `/t1k:debug` |
| Performance regression | `optimizer` | Kit-level command |
| Missing dependency | `implementer` | `/t1k:fix --quick` |

## Recovery Workflow

1. Match error type to role using `errorRecovery` map in routing registry
2. Resolve role to agent (highest-priority registry wins)
3. Dispatch to `/t1k:fix` or `/t1k:debug` as appropriate
4. After fix: always run `/t1k:test` to confirm resolution
5. Never suppress test failures to pass CI

## Stuck Detection (Auto-Escalation)

**After 3+ failed attempts** on the same error with `/t1k:fix` or `/t1k:debug`:

1. Auto-activate `/t1k:problem-solve` — classify the stuck-type and apply matching technique
2. If `mcp__sequential-thinking__sequentialthinking` MCP is available, use it for structured analysis
3. Identify which module owns the stuck task (module-scoped problem analysis)
4. Apply technique, then retry `/t1k:fix` with new approach
5. If still stuck after problem-solving: escalate to user with full analysis

## Error Classification

```
Simple:   single compile error, typo, missing import
Moderate: logic error, wrong API usage, missing config
Complex:  cascading failures, breaking API change, environment issue
```

## Self-Validation (MANDATORY)

AI must verify fixes work BEFORE asking user to test:
1. After any fix: check compilation/run output → confirm zero errors
2. After any test fix: run tests → confirm pass BEFORE reporting to user
3. After any skill update: read the skill file back → verify content is correct

## Skill Sync-Back After Fixes (MANDATORY)

**ALWAYS use the `/t1k:sync-back` and `/t1k:issue` SKILLS — NEVER manually copy files to kit repos or create issues by hand.** These skills handle routing, frontmatter preservation, module path mapping, and PR creation automatically.

After updating any `.claude/skills/` file (gotcha, reference, or SKILL.md):
1. Check for duplicates: `gh pr list --repo {REPO} --search "<skill-name>"` — skip if open PR exists
2. Invoke `/t1k:sync-back --dry-run` to check what changed
3. If changes are generic (not project-specific): invoke `/t1k:sync-back`
4. If fix reveals a skill bug: invoke `/t1k:issue`
