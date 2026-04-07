# Implementation Plan: Mechangelion Full Rebuild

**Created:** 2026-04-07
**Base:** ref-mapped-plan (supersedes mechangelion-rebuild)
**Runtime anchor:** current UnityMechangelion shell (`GameLifetimeScope` -> `MainSceneScope` -> `GameStateMachine`)
**M1 scope:** single `Ordinary` battle loop only
**Deferred after M1:** Boss, PvP, multi-wave, revive economy, advanced presentation polish

---

## Overview

Rebuild gameplay from `MechangelionCrazyGame` while staying inside the current repo contracts:
- `VContainer` root + scene scopes
- `GameStateMachine` + local `IGameState` auto-discovery
- `BaseView` + `BaseScreenPresenter<TView>` screen MVP
- `SignalBus` for gameplay messaging
- `UniTask` for async
- `IGameAssets` for Addressables
- `IHandleUserDataServices` / `UserDataManager` for local data

Important architecture rules:
- Do not add a parallel battle state machine.
- Do not assume a separate `BattleSceneScope` for M1.
- Do not port ref singletons/god objects directly.
- Default all new game code to `Assets/Scripts/`.

---

## Why This Revision Exists

- The earlier full-rebuild draft matched feature ambition, but overreached current runtime reality.
- The current repo only has bootstrap + main scene shell, so M1 must first prove one playable vertical slice.
- Boss and PvP remain planned, but as extensions on top of shared combat/battle/meta foundations.

---

## M1 Playable Slice

M1 is complete when the project supports this loop:

1. Boot from `0.LoadingScene`
2. Enter `1.MainScene`
3. Open home/menu UI
4. Start one ordinary battle from main scene context
5. Player can attack, block, dodge
6. One ordinary enemy can attack and die
7. Battle resolves to victory/defeat
8. Rewards save and home UI refreshes

Non-goals for M1:
- Boss modules / rage system
- PvP mirrored opponent flow
- Multi-wave orchestration
- Dedicated battle scene
- Ads / IAP / CrazyGames / liveops

---

## Phases Summary

| Phase | Name | Effort | Dependencies | Outcome |
|------|------|--------|--------------|---------|
| 0 | Foundation | S (1d) | None | enums, constants, signals grounded in ref |
| 1 | Meta Services | M (2d) | 0 | typed local data + save facade + profile/currency/equipment |
| 2 | Combat Core | S (1d) | 0 | damage, defense, cooldown, registry |
| 3 | Player Combat | M (2d) | 1, 2 | player runtime + combat view |
| 4 | Enemy Ordinary | M (2d) | 2 | ordinary enemy runtime + AI |
| 5 | Battle Orchestration | M (3d) | 3, 4 | battle states inside `GameStateMachine` |
| 6 | HUD + Input | M (2d) | 3, 5 | button-driven battle HUD + input routing |
| 7 | Presentation Core | M (2d) | 3, 4, 5 | minimum animation/VFX/audio feedback |
| 8 | Integration + Result | M (2d) | 5, 6, 7 | home -> battle -> result -> home |
| 9 | Post-M1 Extensions | L | 8 | Boss / PvP / multi-wave / polish |

**Total Estimated Effort for M1:** ~15 days

---

## Extension Path After M1

M2+ can safely add:
- Boss enemy type
- PvP enemy type
- revive flow
- multi-wave encounters
- detachable modules
- richer camera/VFX/audio

To keep that path open, M1 must already preserve:
- `EnemyType` enum including `Boss` and `PvP`
- battle config that can describe mode/type later
- enemy spawning via factory/service, not hardcoded prefab branches in UI/state code
- HUD and battle services that talk about `opponent`, not only `ordinary enemy`

---

## Target Architecture For M1

```text
Assets/Scripts/
|- Core/
|  |- Enums/
|  `- Constants/
|- Features/
|  |- Meta/
|  |- Combat/
|  |- Battle/
|  |- Input/
|  `- Presentation/
|- UI/
|  |- Home/
|  |- Battle/
|  `- Result/
|- Scenes/
|  |- GameLifetimeScope.cs        [MODIFY]
|  `- Main/MainSceneScope.cs      [MODIFY]
|- StateMachines/Game/
|  |- GameStateMachine.cs         [MODIFY]
|  `- States/                     [EXPAND]
`- Models/UserLocalData.cs        [MODIFY]
```

State ownership for M1:
- `GameHomeState`
- `BattlePrepareState`
- `BattleActiveState`
- `BattleVictoryState`
- `BattleDefeatState`
- `BattleResultState`

All of these should implement the existing local `IGameState` contract.

---

## Current Codebase Mapping

- `GameLifetimeScope` owns cross-scene singletons; see `Assets/Scripts/Scenes/GameLifetimeScope.cs`
- `MainSceneScope` owns main-scene runtime registration; see `Assets/Scripts/Scenes/Main/MainSceneScope.cs`
- `GameStateMachine` is the only gameplay state machine for M1; see `Assets/Scripts/StateMachines/Game/GameStateMachine.cs`
- `LoadingScreenPresenter` remains the boot entry; see `Assets/Scripts/Scenes/Screen/LoadingScreenView.cs`
- typed save models should coexist with, then reduce reliance on, `Assets/Scripts/Models/UserLocalData.cs`

---

## Mandatory Conventions

### DI
- Non-`MonoBehaviour`: constructor injection
- `MonoBehaviour`: `[Inject]` method injection only when component must live in scene/prefab
- Root persistent services in `GameLifetimeScope`
- Main-scene battle/runtime services in `MainSceneScope`

### State Management
- Use existing `GameStateMachine`
- New battle flow states implement local `IGameState`
- No second `BattleStateMachine` in M1

### UI
- Use screen MVP for Home / Battle HUD / Result
- Loading bootstrap remains scene-embedded pattern already used by current project

### Persistence
- Use actual `IHandleUserDataServices.Load<T>()` / `Save<T>()`
- Keep fresh save schema for M1
- No fake `GetData/SetData/DeleteData` API in plan or implementation

### Config
- Start M1 with minimal workable config source
- BlueprintFlow is allowed, but do not let config plumbing block the first playable loop
- Hybrid is acceptable: simple bootstrap data first, BlueprintFlow normalization after combat contracts settle

---

## Risks

| Risk | Score | Mitigation |
|------|-------|------------|
| Scope creep from ref gameplay | 16 | lock M1 to one ordinary encounter |
| Architecture drift from current runtime | 15 | reuse `GameStateMachine`, `MainSceneScope`, real framework APIs |
| Config/plumbing slows playable loop | 12 | allow temporary hybrid config |
| Boss/PvP pressure too early | 12 | keep extension hooks, defer full implementation |

---

## Verification Gates

Each implementation phase must pass:
1. Compilation: zero errors
2. DI: services resolve in current scopes
3. Runtime truth: no parallel framework invented when existing contract already exists
4. Testability: pure services can be tested in isolation
5. Playability: every completed phase moves the ordinary battle slice forward

---

## Phase Files

- [Phase 00: Foundation](phase-00-foundation.md)
- [Phase 01: Meta Services](phase-01-meta-services.md)
- [Phase 02: Combat Core](phase-02-combat-core.md)
- [Phase 03: Player Combat](phase-03-player-combat.md)
- [Phase 04: Enemy - Ordinary](phase-04-enemy-ordinary.md)
- [Phase 05: Battle Orchestration](phase-07-battle-orchestration.md)
- [Phase 06: HUD + Input](phase-08-input-hud.md)
- [Phase 07: Presentation Core](phase-09-presentation.md)
- [Phase 08: Integration + Result](phase-10-integration.md)
- [Phase 09: Boss / PvP Extensions](phase-05-enemy-boss.md), (phase-06-enemy-pvp.md)

---

## Commands

```bash
# Start implementation from this plan
/t1k:cook plans/20260407-mechangelion-full-rebuild/

# Verification loop
/t1k:test
/t1k:review
```

---

## Open Decisions

- M1 config source: BlueprintFlow-first or hybrid bootstrap-first?
- Save schema naming for fresh keys
- Whether result/revive should both ship in M1 or revive should move to post-M1
