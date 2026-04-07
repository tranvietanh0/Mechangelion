# Phase 05: Battle Orchestration

**Effort:** M (3 days)
**Dependencies:** Phase 3, Phase 4
**Blocked By:** Phase 3, Phase 4

---

## Objective

Wire the ordinary battle loop into the existing game state architecture.

This phase must:
- use the existing `GameStateMachine`
- add battle-related `IGameState` implementations into the local state list
- run inside `MainSceneScope`

This phase must not:
- create a second `BattleStateMachine`
- require a dedicated `BattleSceneScope`
- require boss/pvp/multi-wave flow to exist before ordinary battle works

---

## Files

```text
Assets/Scripts/Features/Battle/
|- Models/
|  |- BattleContext.cs             [NEW]
|  |- BattleResult.cs              [NEW]
|  `- RewardPayload.cs             [NEW]
|- Services/
|  |- BattleCoordinator.cs         [NEW]
|  |- BattleRewardService.cs       [NEW]
|  `- EnemySpawnService.cs         [NEW]
`- Signals/
   `- optional extra battle signals [NEW/OPTIONAL]

Assets/Scripts/StateMachines/Game/States/
|- BattlePrepareState.cs           [NEW]
|- BattleActiveState.cs            [NEW]
|- BattleVictoryState.cs           [NEW]
|- BattleDefeatState.cs            [NEW]
`- BattleResultState.cs            [NEW]
```

---

## Architecture Notes

### State ownership
- `BattlePrepareState`, `BattleActiveState`, `BattleVictoryState`, `BattleDefeatState`, `BattleResultState` implement the existing local `IGameState`.
- If a state needs machine access, it implements the existing local `IHaveStateMachine` whose property type is `GameStateMachine`.
- `MainSceneScope` already auto-discovers `IGameState`; leverage that instead of introducing a parallel registration model.

### Coordinator ownership
- `BattleCoordinator` should hold battle runtime state for M1.
- It can manage current battle config, current enemy instance, timers, aggregated damage, and final result.
- It should expose commands like:
  - `StartBattle(...)`
  - `FinishBattle(bool victory)`
  - `CleanupBattle()`

### Enemy spawning
- `EnemySpawnService` should spawn one opponent for M1.
- Keep factory/service boundaries so Boss/PvP can plug in later.
- Multi-wave support can be deferred behind future config fields without making M1 depend on it.

---

## DI Registration

Add to `Assets/Scripts/Scenes/Main/MainSceneScope.cs`:

```csharp
builder.DeclareSignal<BattleStartedSignal>();
builder.DeclareSignal<BattleEndedSignal>();

builder.Register<BattleCoordinator>(Lifetime.Scoped).AsInterfacesAndSelf();
builder.Register<BattleRewardService>(Lifetime.Scoped);
builder.Register<EnemySpawnService>(Lifetime.Scoped);
```

Do not introduce `BattleSceneScope` in this phase.

---

## Acceptance Criteria

- Home state can transition into `BattlePrepareState`
- `BattlePrepareState` spawns one ordinary enemy and opens battle HUD
- `BattleActiveState` drives one active encounter
- Victory and defeat transition through the existing `GameStateMachine`
- `BattleResultState` can hand control back to `GameHomeState`

---

## Extension Hooks Preserved

- `BattleContext` may already contain `EnemyType` / `BattleMode`
- `EnemySpawnService` should branch by type later, even if M1 only uses `Ordinary`
- `BattleCoordinator` result model should not assume ordinary-only naming
