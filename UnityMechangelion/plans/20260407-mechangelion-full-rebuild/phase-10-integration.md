# Phase 08: Integration + Result

**Effort:** M (2 days)
**Dependencies:** Phases 5, 6, 7
**Blocked By:** Phases 5, 6, 7

---

## Objective

Integrate the ordinary battle slice into the current main-scene runtime and complete the loop back to home.

This phase should finish:
- home -> battle -> result -> home
- reward apply + save
- DI cleanup inside existing scopes

This phase should not introduce:
- dedicated battle scene
- `BattleSceneScope`
- scene-hopping battle loader for M1

---

## Files

```text
Assets/Scripts/UI/Result/
|- BattleResultView.cs             [NEW]
`- BattleResultPresenter.cs        [NEW]

Assets/Scripts/StateMachines/Game/States/
|- GameHomeState.cs                [MODIFY]
`- BattleResultState.cs            [NEW or refine existing plan state]

Assets/Scripts/Scenes/
|- GameLifetimeScope.cs            [MODIFY]
`- Main/MainSceneScope.cs          [MODIFY]
```

---

## Integration Notes

- `GameHomeState` should be able to trigger battle preparation through UI / service calls.
- `BattleResultPresenter` should show victory/defeat and any applied rewards.
- `BattleRewardService` applies rewards through root meta services.
- After result confirmation, return to `GameHomeState` and refresh home UI from saved/in-memory meta state.

---

## DI Responsibilities

### `GameLifetimeScope`
- root meta singletons
- shared persistent services

### `MainSceneScope`
- battle runtime services
- player/enemy battle runtime services
- input router
- current game states via existing auto-discovery

Do not split M1 integration into a new battle-only scene scope.

---

## Acceptance Criteria

- Project boots through existing loading flow unchanged
- Main scene can open home UI and start one ordinary battle
- Victory applies rewards and returns to home
- Defeat returns to result/home flow cleanly
- No duplicate state-machine architecture introduced
- All registrations map to current codebase entry points

---

## Post-M1 Follow-Up

After this phase is stable, next extension phases may add:
- Boss support
- PvP support
- revive/result branching
- dedicated battle scene only if profiling / content scale later justifies it
