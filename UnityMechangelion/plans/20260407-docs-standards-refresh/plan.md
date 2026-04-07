# Docs + Code Standards Refresh Plan

## Context
- Existing docs: `docs/codebase-summary.md`, `docs/system-architecture.md`, `docs/code-standards.md`
- Related implementation plan: `plans/20260407-mechangelion-rebuild/PLAN.md`
- Inputs: repo analysis, codebase scout, and external documentation research

## Goal
Refresh repo docs so they describe the current Unity project accurately, separate current-state truth from target-state design, and define actionable code standards for the local game layer over shared submodules.

## Why Now
Current docs are useful but drifted on scene loading, signal examples, file organization, Addressables paths, and UI scene contracts. That creates onboarding friction and implementation risk.

## Deliverables
- Updated `docs/codebase-summary.md`
- Updated `docs/system-architecture.md`
- Updated `docs/code-standards.md`
- Clear cross-links to future-state design/plan docs
- Explicit local-vs-shared ownership guidance

## Non-Goals
- No gameplay redesign
- No framework refactor
- No submodule API change
- No automation/tooling changes in this pass

## Phase Summary
| Phase | Status | Focus |
|------|--------|-------|
| 01 | completed | Audit docs, runtime truth, and doc boundaries |
| 02 | completed | Refresh current-state onboarding and architecture docs |
| 03 | completed | Rewrite practical code standards and maintenance triggers |

## Key Decisions
- Keep `docs/system-architecture.md` as current runtime truth
- Keep `docs/architecture-design.md` and rebuild plan as future-state references
- Document Unity-specific contracts explicitly: scenes, `RootUICanvas`, Addressables wrapper, asmdef boundaries
- Prefer short, reviewable standards over generic theory

## Acceptance Criteria
- A new engineer can follow the boot flow from `0.LoadingScene` to `1.MainScene`
- Docs distinguish local game code from shared submodules
- Standards include clear rules for DI, UniTask, SignalBus, screen/state patterns, and Addressables usage
- Known drift items are corrected or explicitly moved to future-state docs

## Risks
- Reintroducing stale assumptions from old docs
- Mixing rebuild design into current-state docs
- Writing standards stricter than the current framework actually supports

## Execution Order
Audit first, then update runtime docs, then finalize standards.

## Phase Files
- `phase-01-doc-audit-ia.md`
- `phase-02-current-state-docs.md`
- `phase-03-code-standards-governance.md`
