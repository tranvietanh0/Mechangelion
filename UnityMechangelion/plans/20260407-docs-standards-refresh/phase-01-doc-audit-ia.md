# Phase 01 - Doc Audit + Information Architecture

## Context links
- `docs/codebase-summary.md`
- `docs/system-architecture.md`
- `docs/code-standards.md`
- `docs/architecture-design.md`
- `plans/20260407-mechangelion-rebuild/PLAN.md`

## Overview
- Date: 2026-04-07
- Priority: High
- Status: Completed

## Key Insights
- The repo is a thin local game layer on top of shared submodules.
- Existing docs mixed onboarding, current-state architecture, and future-state design.
- Drift existed around signal examples, file organization, Addressables paths, and screen/scene contracts.

## Requirements
- Define one source of truth per doc.
- Separate current-state facts from rebuild targets.
- Keep terminology aligned with actual repo structure.

## Architecture
- `docs/codebase-summary.md` = onboarding and repo map
- `docs/system-architecture.md` = runtime architecture and contracts
- `docs/code-standards.md` = enforceable coding guidance
- `docs/architecture-design.md` + rebuild plan = future-state references

## Related code files
- `Assets/Scripts/`
- `Assets/Submodules/GameFoundationCore/`
- `Assets/Submodules/UITemplate/`
- `Assets/Scenes/`
- `Assets/AddressableAssetsData/`

## Implementation Steps
1. Audit existing docs against runtime code.
2. Mark drift items.
3. Assign each topic to the correct doc.
4. Define cross-link boundaries between current-state and target-state docs.

## Todo list
- [x] Audit current docs
- [x] Identify drift items
- [x] Define doc boundaries
- [x] Define cross-link targets

## Success Criteria
- No confusion about which doc describes current runtime truth.
- Every major drift item has an assigned correction target.

## Risk Assessment
- Main risk: carrying forward stale assumptions.
- Mitigation: validate every hard rule against current code before publishing.

## Security Considerations
- Do not document secrets, private credentials, or machine-specific config.

## Next steps
Refresh current-state docs, then finalize practical code standards.
