# Phase 01: Meta Services

**Effort:** M (2 days)
**Dependencies:** Phase 0
**Blocked By:** Phase 0

---

## Objective

Implement typed progression and save/load foundations using the real persistence APIs already present in the codebase.

This phase must support M1 only:
- profile level / xp
- wallet: coins / cores
- owned equipment + equipped slots

This phase must not:
- invent a fake persistence abstraction that does not match framework APIs
- over-model future PvP / boss meta yet

---

## Codebase Constraints

- `IHandleUserDataServices` exposes `Load<T>()`, `Save<T>()`, `SaveAll()`; plan and implementation must use that contract.
- Typed data classes should implement `ILocalData` so they work with the existing user data system.
- Root registration belongs in `GameLifetimeScope`, not in a new custom scene scope.
- `UserLocalData` can remain as compatibility shell while new typed data takes over.

---

## Files

```text
Assets/Scripts/
|- Features/Meta/Models/
|  |- PlayerProfileData.cs        [NEW]
|  |- CurrencyWalletData.cs       [NEW]
|  `- EquipmentInventoryData.cs   [NEW]
|- Features/Meta/Persistence/
|  `- LocalSaveFacade.cs          [NEW]
|- Features/Meta/Services/
|  |- ProfileService.cs           [NEW]
|  |- CurrencyService.cs          [NEW]
|  |- EquipmentService.cs         [NEW]
|  `- UpgradeService.cs           [NEW]
|- Features/Meta/Config/
|  |- EquipmentConfigReader.cs    [NEW/OPTIONAL-FIRST]
|  |- EnemyConfigReader.cs        [NEW/OPTIONAL-FIRST]
|  `- BalanceConfigReader.cs      [NEW/OPTIONAL-FIRST]
`- Models/UserLocalData.cs        [MODIFY]
```

---

## Implementation Notes

### Persistence facade
- `LocalSaveFacade` should wrap the actual async persistence API.
- Preferred shape:
  - `UniTask<T> LoadAsync<T>() where T : class, ILocalData`
  - `UniTask SaveAsync<T>(T data, bool force = false) where T : class, ILocalData`
  - optional `UniTask SaveAllAsync()`

### Services
- `ProfileService`, `CurrencyService`, `EquipmentService` should be root singletons.
- They may implement `IInitializable` to load cached in-memory state once.
- Synchronous convenience methods are fine for read access after initialization.
- If async save is needed, expose explicit `SaveAsync()` methods rather than pretending save is synchronous.

### Config
- If BlueprintFlow reader setup is already straightforward, use it.
- If it slows down M1, use temporary bootstrap config and normalize to BlueprintFlow in a follow-up pass.
- Do not let config readers block meta services from landing.

---

## DI Registration

Add to `Assets/Scripts/Scenes/GameLifetimeScope.cs`:

```csharp
// Progression signals
builder.DeclareSignal<LevelUpSignal>();
builder.DeclareSignal<CurrencyChangedSignal>();
builder.DeclareSignal<EquipmentChangedSignal>();
builder.DeclareSignal<RewardReceivedSignal>();

// Meta services
builder.Register<LocalSaveFacade>(Lifetime.Singleton);
builder.Register<ProfileService>(Lifetime.Singleton).AsInterfacesAndSelf();
builder.Register<CurrencyService>(Lifetime.Singleton).AsInterfacesAndSelf();
builder.Register<EquipmentService>(Lifetime.Singleton).AsInterfacesAndSelf();
builder.Register<UpgradeService>(Lifetime.Singleton);
```

Do not register these in a future battle-only scope for M1.

---

## Acceptance Criteria

- Typed `ILocalData` models load through the real user data service
- Coins / cores / level survive app restart
- Equipment ownership + equipped slots survive app restart
- Services compile against real framework APIs
- `UserLocalData` no longer carries new gameplay state

---

## Future Extensions Kept Open

- PvP ranking / rival profile data can add new typed local models later
- Boss unlock flags can extend meta models later
- Save migration can be added later without rewriting core services
