---

origin: theonekit-core
repository: The1Studio/theonekit-core
module: null
protected: true
---
# MCP Server Requirements

## Tiered MCP Requirements

TheOneKit declares MCP server requirements in `t1k-config-*.json` → `mcp` section. Each kit layer can declare its own requirements. Core defines the baseline.

### Tier Definitions

| Tier | Meaning | Behavior |
|------|---------|----------|
| **required** | Must be connected for core workflows to function | SessionStart hook warns loudly; doctor check fails |
| **recommended** | Significantly improves workflow quality | SessionStart hook suggests installation |
| **optional** | Nice to have for specific use cases | Mentioned in `/t1k:help` output only |

### Core Layer MCPs (t1k-config-core.json)

| MCP Server | Tier | Used By | Purpose |
|------------|------|---------|---------|
| `github` | required | t1k:issue, t1k:triage, t1k:sync-back, t1k:git pr | GitHub issue/PR management |
| `context7` | required | t1k:plan, t1k:cook, t1k:ask, t1k:brainstorm | Library/framework documentation lookup |
| `sequential-thinking` | recommended | t1k:problem-solve, error-recovery | Structured step-by-step analysis when stuck |
| `memory` | recommended | Cross-session knowledge persistence | Knowledge graph for entity tracking |
| `serena` | optional | Semantic code navigation | Go-to-definition, find-usages, symbol overview |
| `playwright` | optional | Frontend testing, browser automation | E2E testing, screenshots, visual regression |
| `chrome-devtools` | optional | Frontend debugging | Console, network, performance analysis |

### Kit Layer MCPs (engine kits declare their own)

Engine kits extend the MCP requirements in their own `t1k-config-{kit}.json`:

```json
{
  "mcp": {
    "required": [
      { "name": "UnityMCP", "installCmd": "claude mcp add UnityMCP -- uvx mcp-for-unity", "purpose": "Unity Editor bridge" }
    ]
  }
}
```

## Auto-Setup Protocol

### SessionStart Hook (`check-mcp-health.cjs`)

On every session start:
1. Read ALL `t1k-config-*.json` → collect `mcp.required` and `mcp.recommended` entries
2. Run `claude mcp list` to get connected servers
3. For each **required** MCP not connected:
   - Output: `[t1k:mcp-missing] REQUIRED: {name} — {purpose}. Install: {installCmd}`
4. For each **recommended** MCP not connected:
   - Output: `[t1k:mcp-suggest] RECOMMENDED: {name} — {purpose}. Install: {installCmd}`
5. AI reads these outputs and offers to install missing MCPs

### AI Response Protocol

When the AI sees `[t1k:mcp-missing]` or `[t1k:mcp-suggest]` outputs:

1. **Required missing:** Warn user prominently, offer to install via `installCmd`
2. **Recommended missing:** Mention briefly, offer installation
3. **Batch installs:** If multiple missing, offer to install all at once
4. **Don't repeat:** Track offered MCPs per session — don't re-offer after user declines
5. **Auth-required MCPs:** If install succeeds but needs auth, instruct user to run `! claude mcp auth {name}`

### Doctor Check

`/t1k:doctor` includes MCP validation:
- Check all `required` MCPs are connected
- Warn about missing `recommended` MCPs
- Report: `MCP health: {N}/{total} required connected, {M} recommended missing`

## Install Commands Reference

| MCP Server | Install Command |
|------------|----------------|
| `github` | `claude mcp add github` (built-in, usually auto-configured) |
| `context7` | `claude mcp add context7 -- npx -y @context7/mcp` or use HTTP: `https://mcp.context7.com/mcp` |
| `sequential-thinking` | `claude mcp add sequential-thinking -- npx -y @modelcontextprotocol/server-sequential-thinking` |
| `memory` | `claude mcp add memory -- npx -y @modelcontextprotocol/server-memory` |
| `serena` | `claude mcp add serena -- serena start-mcp-server --context ide-assistant --enable-web-dashboard false` |
| `playwright` | `claude mcp add playwright -- npx @playwright/mcp@latest --headless` |
| `chrome-devtools` | `claude mcp add chrome-devtools -- npx -y chrome-devtools-mcp@latest` |

## Config Schema

```json
{
  "mcp": {
    "required": [
      {
        "name": "github",
        "purpose": "GitHub issue/PR management for triage, sync-back, issue reporting",
        "installCmd": "claude mcp add github"
      }
    ],
    "recommended": [
      {
        "name": "sequential-thinking",
        "purpose": "Structured analysis for problem-solving when stuck",
        "installCmd": "claude mcp add sequential-thinking -- npx -y @modelcontextprotocol/server-sequential-thinking"
      }
    ],
    "optional": [
      {
        "name": "serena",
        "purpose": "Semantic code navigation (go-to-def, find-usages)",
        "installCmd": "claude mcp add serena -- serena start-mcp-server --context ide-assistant"
      }
    ]
  }
}
```
