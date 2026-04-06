---

origin: theonekit-core
repository: The1Studio/theonekit-core
module: null
protected: true
---
# TheOneKit Skill Activation

## Activation Algorithm

**Step 1 — Load activation sources:**
Read ALL `.claude/t1k-activation-*.json` files (CI-generated, released in module ZIPs).
Also read `module.json` → `activation` field for each installed module (SSOT for per-module activation).

**Step 2 — Match keywords (ADDITIVE):**
For every source, check the request/file context against `keywords`/`mappings` arrays.
Collect ALL matching skills across ALL sources — do not stop at first match.
Only activate skills from **installed** modules (check `.claude/metadata.json` → `installedModules`).

**Step 3 — Activate collected skills:**
Activate every skill in the collected set.

**Step 4 — Session baseline:**
Read required modules' `module.json` → `activation.sessionBaseline`.
Also read `t1k-activation-*.json` → `sessionBaseline` entries.
Activate baseline skills regardless of keyword match.

**Fallback:** If no activation sources exist, activate no automatic skills. Module installs provide the sources.

## Fragment Format

Each `t1k-activation-*.json` file follows this schema:

```json
{
  "registryVersion": 1,
  "kitName": "example-kit",
  "priority": 20,
  "sessionBaseline": ["skill-a", "skill-b"],
  "mappings": [
    {
      "keywords": ["keyword1", "keyword2"],
      "skills": ["skill-name-1", "skill-name-2"]
    }
  ]
}
```

## Core Principle

Activation is ADDITIVE — never exclusive. Higher-priority fragments do not suppress lower-priority ones. Every matched skill from every fragment is activated.

## Example

Given two fragments:
- `t1k-activation-core.json` maps "auth" → ["jwt-skill"]
- `t1k-activation-mykit.json` maps "auth" → ["mykit-auth-skill"]

A request containing "auth" activates BOTH: `jwt-skill` AND `mykit-auth-skill`.

## Deduplication

If the same skill appears in multiple fragments, activate it only once.
