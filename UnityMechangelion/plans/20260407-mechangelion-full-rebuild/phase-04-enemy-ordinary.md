# Phase 04: Enemy - Ordinary

**Effort:** M (2 days)
**Dependencies:** Phase 2
**Blocked By:** Phase 2

---

## Objective

Implement the first opponent type: ordinary enemy.

This phase must provide:
- one enemy base runtime contract
- one ordinary enemy implementation
- one enemy spawn/factory path that M1 can use

This phase should intentionally leave room for:
- boss subclass/strategy later
- PvP subclass/strategy later

---

## Files

```text
Assets/Scripts/Features/Combat/Enemy/
|- Base/
|  |- EnemyBase.cs
|  |- EnemyView.cs
|  |- EnemyStats.cs
|  `- EnemyFactory.cs
`- Ordinary/
   |- OrdinaryEnemyAI.cs
   `- OrdinaryEnemyBehavior.cs
```

---

## Architecture Notes

### Enemy factory role
- `EnemyFactory` should be a runtime creation boundary, not a full content database.
- For M1 it only needs to reliably create one ordinary enemy.
- Keep factory branching by `EnemyType`, but return ordinary only in M1 runtime use.

### Config
- If `EnemyConfigReader` is available and stable, use it.
- Otherwise allow a small bootstrap config source for the first ordinary enemy so M1 does not block.

### Signals
- Enemy attack signalling is valid, but keep the payload minimal.
- Only add `EnemyAttackSignal` if battle resolution truly needs it.

### Lifetime
- Enemy runtime belongs to `MainSceneScope` encounter services.
- `EnemyFactory` should therefore be scoped to the main scene for M1, not root singleton.

---

## DI Registration

Add to `Assets/Scripts/Scenes/Main/MainSceneScope.cs`:

```csharp
builder.DeclareSignal<EnemyAttackSignal>();
builder.Register<EnemyFactory>(Lifetime.Scoped);
```

This keeps the enemy creation pipeline aligned with battle runtime state.

---

## Acceptance Criteria

- ordinary enemy can be spawned and initialized for one encounter
- ordinary enemy can attack using shared combat services/contracts
- ordinary enemy can receive damage, die, and cleanly unregister from runtime registry
- factory/service boundaries remain valid for later Boss/PvP additions

---

## Extension Hooks Preserved

- boss and PvP can plug into `EnemyFactory` via `EnemyType`
- `EnemyBase` should define only the truly shared contract
- battle flow should talk to spawned opponent through neutral contracts, not ordinary-specific UI assumptions
