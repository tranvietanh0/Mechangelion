# Phase 03 - Code Standards + Governance

## Context links
- `docs/code-standards.md`
- `docs/codebase-summary.md`
- `docs/system-architecture.md`
- `Assets/Submodules/GameFoundationCore/Scripts/GameFoundationVContainer.cs`
- `Assets/Submodules/GameFoundationCore/Scripts/UIModule/ScreenManagerVContainer.cs`

## Overview
- Date: 2026-04-07
- Priority: High
- Status: Completed

## Key Insights
- Existing standards had useful intent but several rules were too absolute or stale.
- The repo needs standards tied to actual patterns: constructor DI, `UniTask`, `SignalBus`, `IGameAssets`, screen/presenter naming, and local-vs-shared ownership.
- Unity-specific scene contracts and doc maintenance triggers were missing.

## Requirements
- Keep standards review-ready and specific.
- Separate hard rules from preferred structure.
- Ensure examples match current repo behavior.

## Architecture
- Standards now cover: scope, naming, file placement, DI, async, signals, screen flow, state flow, Addressables usage, scene contracts, and review checklist.

## Related code files
- `Assets/Scripts/**/*.cs`
- `Assets/Submodules/GameFoundationCore/**/*.cs`
- `Assets/Submodules/UITemplate/**/*.cs`

## Implementation Steps
1. Restructure the standards doc around current repo behavior.
2. Correct stale guidance.
3. Add practical rules for DI, async, signals, screens, states, and Addressables.
4. Add maintenance triggers tied to architecture changes.

## Todo list
- [x] Restructure standards doc
- [x] Correct stale rules
- [x] Add architecture-specific rules
- [x] Add review checklist
- [x] Add doc maintenance triggers

## Success Criteria
- Reviewers can use the standards doc directly during implementation and PR review.
- Standards do not conflict with current framework/runtime behavior.

## Risk Assessment
- Risk: codifying a false rule and spreading it through reviews.
- Mitigation: validate every hard rule against live code before finalizing.

## Security Considerations
- Standards must discourage secret literals, unsafe logging, and undocumented external endpoints.

## Next steps
If runtime architecture changes during rebuild, refresh these docs in the same change set.
