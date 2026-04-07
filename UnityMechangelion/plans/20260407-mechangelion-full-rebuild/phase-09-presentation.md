# Phase 07: Presentation Core

**Effort:** M (2 days)
**Dependencies:** Phase 3, Phase 4, Phase 5
**Blocked By:** Phase 3, Phase 4, Phase 5

---

## Objective

Add only the minimum presentation feedback needed to make the ordinary battle slice readable and satisfying.

Priority order:
1. player/enemy hit feedback
2. attack / block / dodge animation triggering
3. basic sound feedback
4. optional lightweight VFX

Defer advanced systems until after M1:
- dynamic battle camera rig
- large pooled VFX library
- boss-specific presentation layers
- bespoke audio variation systems

---

## Files

```text
Assets/Scripts/Features/Presentation/
|- Animation/
|  |- CombatAnimationController.cs  [NEW]
|  `- AnimationHashes.cs            [NEW]
|- VFX/
|  `- CombatVfxService.cs           [NEW/OPTIONAL-FIRST]
`- Audio/
   `- CombatAudioService.cs         [NEW/OPTIONAL-FIRST]
```

Optional after M1:
- camera controller
- pooled generic VFX framework
- animation relay helpers for more complex rigs

---

## Architecture Notes

- Hook presentation to combat/battle signals where that helps decoupling.
- Keep `PlayerCombatView` / `EnemyView` responsible for direct animation triggering on their own actors.
- Add service-level VFX/audio only when it clearly reduces duplication.
- Use actual `AudioService` API already in the framework.

---

## Acceptance Criteria

- player attack/block/dodge visibly trigger
- enemy attack/hit/death visibly trigger
- battle feels readable without advanced camera work
- presentation layer does not become a prerequisite for Boss/PvP work later

---

## Extension Hooks Preserved

- boss rage/module effects can plug into the same signal flow later
- PvP can reuse the same hit/block/dodge presentation primitives
