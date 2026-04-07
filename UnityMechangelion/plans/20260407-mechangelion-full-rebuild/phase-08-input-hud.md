# Phase 06: HUD + Input

**Effort:** M (2 days)
**Dependencies:** Phase 3, Phase 5
**Blocked By:** Phase 3, Phase 5

---

## Objective

Implement a battle HUD and route player actions through UI-driven input first.

For M1, prefer the safest path:
- on-screen HUD buttons / explicit actions
- hold-release for attack if needed
- block press/release
- dodge left/right buttons

Do not make custom full-screen touch-zone routing a requirement for first playable.

---

## Files

```text
Assets/Scripts/UI/Battle/
|- BattleHudView.cs                [NEW]
|- BattleHudPresenter.cs           [NEW]
|- CombatActionButtonView.cs       [NEW]
`- Models/BattleHudModel.cs        [NEW]

Assets/Scripts/Features/Input/
`- CombatInputRouter.cs            [NEW]
```

Optional later:
- `TouchInputHandler.cs`
- configurable touch zones
- gesture/swipe routing

---

## Architecture Notes

- `BattleHudPresenter` should be a normal MVP screen presenter.
- Presenter listens to combat/battle signals and updates health, cooldown, and state indicators.
- `CombatInputRouter` should translate HUD button intents into player runtime commands.
- Keep router API neutral enough so touch-zone input can be added later without changing battle services.

Recommended command surface:
- `StartPrepareAttack(bool isRightHand)`
- `ReleasePreparedAttack()`
- `StartBlock()`
- `StopBlock()`
- `DodgeLeft()`
- `DodgeRight()`

---

## DI Registration

Add to `MainSceneScope`:

```csharp
builder.Register<CombatInputRouter>(Lifetime.Scoped).AsInterfacesAndSelf();
```

HUD presenter remains screen-manager owned.

---

## Acceptance Criteria

- Player can attack from HUD input
- Player can block and release block from HUD input
- Player can dodge left/right from HUD input
- HUD reflects player health and opponent health
- HUD reflects cooldown state without requiring a separate battle scene

---

## Extension Hooks Preserved

- `CombatInputRouter` can later accept touch-zone or gesture sources
- HUD model should refer to `opponent`, not `ordinary enemy`
- PvP can reuse the same HUD and router later
