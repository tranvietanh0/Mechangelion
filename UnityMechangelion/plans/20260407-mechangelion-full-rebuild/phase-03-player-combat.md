# Phase 03: Player Combat

**Effort:** M (2 days)
**Dependencies:** Phase 1, Phase 2
**Blocked By:** Phase 1, Phase 2

---

## Objective

Implement the player-side combat runtime used by the M1 ordinary battle loop.

This phase covers:
- player combat state and stats
- attack / block / dodge commands
- player animation/view bridge
- integration with equipment/meta services

---

## Files

```text
Assets/Scripts/Features/Combat/Player/
|- PlayerCombatController.cs
|- PlayerCombatView.cs
|- PlayerCombatStats.cs
`- PlayerActionState.cs
```

Optional later:
- armor/shield components if they clearly need their own behaviours

---

## Architecture Notes

### Scene ownership
- `PlayerCombatController` is a scene/prefab `MonoBehaviour` injected from `MainSceneScope`.
- Avoid fragile `FindObjectOfType` build callbacks as the main plan path.
- Prefer one of these patterns:
  - register component in hierarchy if the battle root is scene-owned
  - instantiate a prefab through a battle coordinator/factory and inject via resolver

### Runtime responsibilities
- `PlayerCombatController` owns transient action state.
- `PlayerCombatView` owns animation-facing behaviour only.
- Damage math belongs to combat services, not the view.
- Equipment/meta services only provide stats and equipment data; they do not own combat runtime.

### M1 simplification
- Only implement the actions the ordinary encounter needs.
- If left/right attack symmetry complicates first playable too much, keep the API shape but allow both hands to share the same internal implementation initially.

---

## DI Registration

Register supporting services in `MainSceneScope`.

For the player runtime component, choose one of these concrete implementation paths during build phase:
- scene-owned player combat root + `RegisterComponentInHierarchy<PlayerCombatController>()`
- prefab-instantiated player root via battle coordinator + resolver injection

The plan requirement is:
- no `FindObjectOfType`-driven glue as the core architecture

---

## Acceptance Criteria

- player runtime can be prepared/reset for a new encounter
- player can attack, block, and dodge through command calls
- player stats are derived from base constants + equipped gear
- player can receive damage and emit defeated signal when dead
- view/controller separation stays clean enough for later Boss/PvP reuse

---

## Extension Hooks Preserved

- controller APIs remain reusable by HUD buttons now and touch gestures later
- PvP can mirror or reuse the same command semantics later
- presentation systems can attach to player signals without changing combat rules
