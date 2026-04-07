# Phase 02: Combat Core

**Effort:** S (1 day)
**Dependencies:** Phase 0
**Blocked By:** Phase 0

---

## Objective

Implement the pure combat logic used by all future encounter types.

This is the platform layer for:
- ordinary enemy battles in M1
- boss damage flow later
- PvP mirrored combat later

---

## Files

```text
Assets/Scripts/Features/Combat/
|- Models/
|  |- CombatStats.cs
|  |- DamageRequest.cs
|  `- DamageResult.cs
`- Services/
   |- DamageCalculatorService.cs
   |- CooldownService.cs
   |- DefenseResolverService.cs
   |- DifficultyService.cs
   `- CombatRegistry.cs
```

---

## Architecture Notes

### Purity
- These services should be mostly framework-agnostic.
- `CooldownService` may implement `ITickable` because the current project already uses VContainer-managed ticking.
- Avoid spreading `Random`-driven gameplay everywhere; keep random crit or variance centralized.

### Registry role
- `CombatRegistry` is a runtime lookup used by battle/HUD/services during M1.
- It should not become a save system.
- It should be safe to clear between encounters.

### Difficulty
- `DifficultyService` should expose only what M1 needs: ordinary enemy level/stat adjustment and reward multiplier support.
- Boss/PvP-specific balancing can extend this later.

---

## DI Registration

Add to `Assets/Scripts/Scenes/Main/MainSceneScope.cs`:

```csharp
builder.Register<DamageCalculatorService>(Lifetime.Scoped);
builder.Register<CooldownService>(Lifetime.Scoped).AsInterfacesAndSelf();
builder.Register<DefenseResolverService>(Lifetime.Scoped);
builder.Register<DifficultyService>(Lifetime.Scoped);
builder.Register<CombatRegistry>(Lifetime.Scoped);
```

Reasoning:
- these are encounter/runtime services for M1, not app-lifetime persistence services
- keeping them scoped to the main scene avoids cross-session stale runtime state

---

## Guardrails

- Do not hardcode ordinary-enemy-only assumptions into models like `DamageRequest` / `CombatStats`.
- Do not make `CombatRegistry` know about UI or state transitions.
- Do not let this phase depend on config readers being finished.

---

## Acceptance Criteria

- Damage can be calculated from an attacker request and target stats
- Defense/block/dodge resolution works independently from MonoBehaviours
- Cooldowns tick and report progress correctly
- Runtime registry can track player/opponent combatants for M1
- Services are reusable by future Boss/PvP extensions without redesign
