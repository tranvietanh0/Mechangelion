# Phase 02 - Refresh Current-State Docs

## Context links
- `docs/codebase-summary.md`
- `docs/system-architecture.md`
- `Assets/Scripts/Scenes/GameLifetimeScope.cs`
- `Assets/Scripts/Scenes/Loading/LoadingSceneScope.cs`
- `Assets/Scripts/Scenes/Screen/LoadingScreenView.cs`
- `Assets/Scripts/Scenes/Main/MainSceneScope.cs`
- `Assets/Scripts/StateMachines/Game/GameStateMachine.cs`

## Overview
- Date: 2026-04-07
- Priority: High
- Status: Completed

## Key Insights
- The real boot path is `0.LoadingScene` -> user data load -> Addressable `1.MainScene`.
- The loading screen is manually initialized from the scene, not loaded like a normal addressable screen.
- `RootUICanvas` is a runtime scene contract and must be documented explicitly.

## Requirements
- Update onboarding to match actual entry flow.
- Clarify local-vs-shared ownership.
- Fix inaccurate Addressables and signal guidance.

## Architecture
- `docs/codebase-summary.md` stays short and task-oriented.
- `docs/system-architecture.md` captures runtime flow, scopes, screen flow, signals, data flow, and doc boundaries.

## Related code files
- `Assets/Scenes/0.LoadingScene.unity`
- `Assets/Scenes/1.MainScene.unity`
- `Assets/Submodules/GameFoundationCore/Scripts/UIModule/ScreenFlow/Manager/ScreenManager.cs`
- `Assets/Submodules/GameFoundationCore/Scripts/UIModule/ScreenFlow/Manager/SceneDirector.cs`
- `Assets/Submodules/GameFoundationCore/Scripts/AssetLibrary/GameAssets.cs`

## Implementation Steps
1. Rewrite quick-start and repo map.
2. Add exact boot/runtime flow.
3. Document manual-init screen pattern and `RootUICanvas` dependency.
4. Clarify current-state vs future-state docs.

## Todo list
- [x] Update onboarding
- [x] Update repo map
- [x] Document boot flow
- [x] Document screen and scene contracts
- [x] Add doc boundary notes

## Success Criteria
- A new engineer can understand the current boot/runtime flow without reverse-engineering scene setup.
- Docs no longer contradict actual Addressables or screen initialization behavior.

## Risk Assessment
- Risk: over-documenting volatile implementation details.
- Mitigation: document contracts and flow, not line-level internals.

## Security Considerations
- Keep docs architecture-focused; avoid environment-specific private settings.

## Next steps
Lock code standards to the same runtime truth and add maintenance triggers.
