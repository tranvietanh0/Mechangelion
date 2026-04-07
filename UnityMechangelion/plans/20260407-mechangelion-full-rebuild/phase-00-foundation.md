# Phase 00: Foundation

**Effort:** S (1 day)
**Dependencies:** None
**Blocked By:** None

---

## Objective

Create the minimum shared types needed by later phases without inventing architecture early.

This phase should only introduce:
- shared enums
- shared combat constants extracted from ref
- gameplay signal payloads used by later services/states

---

## Files

```text
Assets/Scripts/Core/
|- Enums/
|  |- WeaponType.cs
|  |- EnemyType.cs
|  |- EquipmentSlotType.cs
|  |- CurrencyType.cs
|  `- BattleMode.cs                 [NEW]
`- Constants/
   `- CombatConstants.cs

Assets/Scripts/Features/Signals/
|- CombatSignals.cs
`- ProgressionSignals.cs
```

---

## Architecture Notes

- Keep `EnemyType` values for `Ordinary`, `Boss`, `PvP` from the start so future phases do not require enum churn.
- Add a lightweight `BattleMode` enum now if useful for future extension, even though M1 only uses one ordinary battle flow.
- Signals remain plain POCO payloads; declaration happens later in scopes where they are used.

---

## Guardrails

- Do not add interfaces here.
- Do not add data readers/config readers here.
- Do not let future Boss/PvP cases force extra complexity into M1 foundation types.

---

## Acceptance Criteria

- Shared enums cover M1 plus deferred Boss/PvP extension points
- Combat constants reflect the ref values actually needed by M1
- Signal payloads are enough for combat, battle result, and progression refresh
- No phase after this needs to redefine shared primitive types
