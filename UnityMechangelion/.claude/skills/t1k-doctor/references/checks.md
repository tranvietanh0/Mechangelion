---

origin: theonekit-core
repository: The1Studio/theonekit-core
module: null
protected: true
---
# Doctor Checks Reference

## Core Checks (#1–6)

1. **Role coverage** — every role in `t1k-routing-*.json` has a matching agent `.md` file
2. **Skill existence** — every skill in `t1k-activation-*.json` has a matching skill folder in `.claude/skills/`
3. **No cross-layer hardcoding** — scan `t1k-routing-*.json` values for engine-specific strings (dots-, unity-, cocos-)
4. **Manifest integrity** — `.t1k-manifest.json` matches actually installed files
5. **Registry version compat** — all `t1k-routing-*.json` and `t1k-activation-*.json` use `registryVersion: 1`
6. **Config completeness** — every command in `t1k-config-*.json` has a matching skill folder

## Module Checks (#7–17)

Follow protocol: `rules/module-detection-protocol.md` — skip if no `installedModules` key or no metadata.

| # | Check | Validates |
|---|---|---|
| 7 | Module file ownership | Every skill file belongs to exactly one module via `.t1k-manifest.json` (no overlap) |
| 8 | Module dependency integrity | All declared dependencies (from module.json) are installed with compatible versions |
| 9 | Activation fragment match | Each installed module has activation source (module.json or t1k-activation-*.json) |
| 10 | Module agent presence | Each module declaring agents has matching `.md` files |
| 11 | Routing overlay validity | Module overlays reference only that module's agents |
| 12 | No stale module files | No files from uninstalled modules remain (cross-check manifests) |
| 13 | SessionBaseline in required module | `sessionBaseline` skills are in required modules only |
| 14 | Keyword uniqueness | No keyword maps to skills in two different modules |
| 15 | Routing priority uniqueness | No two module overlays override same role at same priority |
| 16 | Origin frontmatter match | In-file `origin` frontmatter matches metadata entry |
| 17 | Module frontmatter presence | Files in `modules/*/` have `module:` field in frontmatter matching parent dir |

## Manifest Checks (#21)

| # | Check | Validates |
|---|---|---|
| 21 | Module manifest integrity | Each installed module has `modules/{name}/manifest.json`; listed files exist at flat locations; no orphaned flat files |

**Check #21 details:**
1. For each installed module in metadata: verify `.claude/modules/{name}/manifest.json` exists
2. For each file in manifest: verify it exists at the flattened location
3. Scan `.claude/skills/` for dirs matching `{module}-*` pattern not in any manifest → orphaned
4. Severity: WARN (pre-flattening installs won't have manifests)

## SSOT & Structure Checks (#22–27)

| # | Check | Validates |
|---|---|---|
| 22 | schemaVersion present | `metadata.json` has `schemaVersion: 3` |
| 23 | Version presence | `metadata.json` has real `version` (not `"0.0.0-source"`) and `buildDate` (not `null`) |
| 24 | No stale root modules/ | No `modules/` at repo root alongside `.claude/modules/` (canonical) |
| 25 | Context requiredPaths set | Engine kits (unity/cocos/rn) have `context.requiredPaths` in config |
| 26 | Activation format modern | All `t1k-activation-*.json` use `mappings` array, not deprecated `keywords` object |
| 27 | v3 installedModules | CLI writes `installedModules` with `kit`, `repository`, `version` per module |

## No-Override Checks (#28–29)

| # | Check | Validates |
|---|---|---|
| 28 | Filename collision detection | No two installed kits/modules have same-named agents, skills, or rules. Group files by basename + read `origin` metadata. Exception: merge targets (metadata.json, t1k-modules.json, settings.json, CLAUDE.md). |
| 29 | Agent prefix correctness | Non-core agents have proper prefix: `{kit-short}-` (kit-wide) or `{kit-short}-{module}-` (module). Core agents have no prefix. |

**Check #28 details:**
1. Walk `.claude/agents/`, `.claude/skills/`, `.claude/rules/`
2. Read each file's `origin` metadata (frontmatter/`_origin`)
3. Group files by basename; if same basename with different `origin` values → ERROR: collision
4. Fix mode: suggest running CI auto-prefix or manual rename

**Check #29 details:**
1. For each agent in `.claude/agents/`, read `origin` field — derive expected kit-short
2. If origin != core: verify filename starts with `{kit-short}-`
3. If module agents: verify filename starts with `{kit-short}-{module}-`

## Frontmatter Quality Checks (#18–20)

| # | Check | Validates |
|---|---|---|
| 18 | Agent maxTurns presence | Every agent `.md` has `maxTurns:` in frontmatter |
| 19 | Skill effort presence | Every skill `SKILL.md` has `effort:` in frontmatter (low/medium/high) |
| 20 | Agent model appropriateness | Implementer/debugger agents should use `inherit` or `opus`; utility agents (git, docs) should use `sonnet` |

### Frontmatter Check Output
```
### Frontmatter Quality
- Agent maxTurns: [PASS | WARN — N agents missing maxTurns: {list}]
- Skill effort: [PASS | WARN — N skills missing effort: {list}]
- Agent model: [PASS | WARN — {agent} uses {model} but role suggests {recommended}]
```
