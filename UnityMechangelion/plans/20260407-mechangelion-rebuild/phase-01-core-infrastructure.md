# Phase 01: Core + Infrastructure

**Effort:** S (1 day)
**Dependencies:** None
**Blocked By:** None

---

## Objective

Tạo foundation layer: interfaces, enums, constants, signals, và infrastructure wrappers.

---

## File Ownership

```
Assets/Scripts/
├── Core/
│   ├── Interfaces/
│   │   ├── IEntity.cs              [NEW]
│   │   ├── IDamageable.cs          [NEW]
│   │   ├── IAttacker.cs            [NEW]
│   │   ├── IBlockable.cs           [NEW]
│   │   ├── IDodgeable.cs           [NEW]
│   │   └── IUpgradeable.cs         [NEW]
│   │
│   ├── Data/
│   │   ├── Enums/
│   │   │   ├── WeaponType.cs       [NEW]
│   │   │   ├── EnemyType.cs        [NEW]
│   │   │   ├── EquipmentSlot.cs    [NEW]
│   │   │   ├── CurrencyType.cs     [NEW]
│   │   │   └── MissionHardType.cs  [NEW]
│   │   │
│   │   └── Constants/
│   │       ├── CombatConstants.cs  [NEW]
│   │       └── BalanceConstants.cs [NEW]
│   │
│   └── Signals/
│       ├── CombatSignals.cs        [NEW]
│       ├── ProgressionSignals.cs   [NEW]
│       └── GameStateSignals.cs     [NEW]
│
└── Infrastructure/
    └── Persistence/
        ├── ISaveService.cs         [NEW]
        └── SaveService.cs          [NEW]
```

**Total Files:** 15

---

## Implementation Details

### 1. Core Interfaces

```csharp
// IEntity.cs
public interface IEntity
{
    string Id { get; }
    bool IsActive { get; }
}

// IDamageable.cs
public interface IDamageable
{
    float CurrentHealth { get; }
    float MaxHealth { get; }
    bool IsDead { get; }
    void TakeDamage(DamageResult damage);
    event Action<DamageResult> OnDamageTaken;
}

// IAttacker.cs
public interface IAttacker
{
    DamageData GetBaseDamage();
    bool CanAttack { get; }
}

// IBlockable.cs
public interface IBlockable
{
    bool IsBlocking { get; }
    float BlockReduction { get; }
    bool TryBlock();
}

// IDodgeable.cs
public interface IDodgeable
{
    bool IsDodging { get; }
    void Dodge(DodgeDirection direction);
}

// IUpgradeable.cs
public interface IUpgradeable
{
    int Level { get; }
    bool CanUpgrade { get; }
    void Upgrade();
}
```

### 2. Enums

```csharp
// WeaponType.cs
public enum WeaponType { Melee, Gun, MachineGun }

// EnemyType.cs
public enum EnemyType { Ordinary, Minion, Boss, PvP, Small }

// EquipmentSlot.cs
public enum EquipmentSlot { ArmorTop, ArmorBottom, MeleeLeft, MeleeRight, Ranged, Shield }

// CurrencyType.cs
public enum CurrencyType { Coins, Cores }

// MissionHardType.cs
public enum MissionHardType { Easy, Medium, Hard }

// DodgeDirection.cs (add to Enums folder)
public enum DodgeDirection { Left, Right }
```

### 3. Constants

```csharp
// CombatConstants.cs
public static class CombatConstants
{
    public const float PunchPrepareTime = 0.75f;
    public const float PunchExecuteTime = 0.167f;
    public const float PunchReturnTime = 0.833f;
    public const float PunchReloadTime = 1f;
    public const float BlockReloadTime = 4f;
    public const float DodgeReloadTime = 1f;
    public const float SuperBlockWindow = 0.35f;
    public const float BaseDamage = 0.1f;
}

// BalanceConstants.cs
public static class BalanceConstants
{
    public const float EnemyHealthMultiplierLow = 0.5f;
    public const float EnemyHealthMultiplierMid = 0.75f;
    public const float EnemyHealthMultiplierHigh = 0.8f;
    public const float EnemyDamageMultiplier = 0.625f;
    public const int UpgradeBaseCost = 100;
    public const int UpgradeIncreaseEvery = 3;
    public const int UpgradeMaxCost = 1000;
}
```

### 4. Signals

```csharp
// CombatSignals.cs
public class DamageDealtSignal
{
    public IAttacker Attacker { get; set; }
    public IDamageable Target { get; set; }
    public DamageResult Damage { get; set; }
}

public class BlockedSignal
{
    public IBlockable Blocker { get; set; }
    public float BlockedAmount { get; set; }
}

public class EntityDefeatedSignal
{
    public IDamageable Entity { get; set; }
    public bool IsPlayer { get; set; }
}

// ProgressionSignals.cs
public class LevelUpSignal
{
    public int NewLevel { get; set; }
    public int OldLevel { get; set; }
}

public class CurrencyChangedSignal
{
    public CurrencyType Type { get; set; }
    public int OldAmount { get; set; }
    public int NewAmount { get; set; }
}

public class EquipmentUpgradedSignal
{
    public string EquipmentId { get; set; }
    public int NewLevel { get; set; }
}

// GameStateSignals.cs
public class BattleStartedSignal
{
    public string EnemyId { get; set; }
    public MissionHardType Difficulty { get; set; }
}

public class BattleEndedSignal
{
    public bool Victory { get; set; }
    public float Duration { get; set; }
}
```

### 5. Infrastructure

```csharp
// ISaveService.cs
public interface ISaveService
{
    T Load<T>(string key, T defaultValue = default);
    void Save<T>(string key, T value);
    bool HasKey(string key);
    void DeleteKey(string key);
    void DeleteAll();
}

// SaveService.cs
public class SaveService : ISaveService
{
    private readonly IHandleUserDataServices userDataServices;
    
    public SaveService(IHandleUserDataServices userDataServices)
    {
        this.userDataServices = userDataServices;
    }
    
    public T Load<T>(string key, T defaultValue = default)
    {
        // Wrap IHandleUserDataServices
    }
    
    public void Save<T>(string key, T value)
    {
        // Wrap IHandleUserDataServices
    }
    
    // ... other methods
}
```

---

## Verification Checklist

- [ ] All interfaces compile without errors
- [ ] All enums have correct values matching gameplay docs
- [ ] Constants match values from MechangelionCrazyGame
- [ ] Signals are classes with properties
- [ ] SaveService correctly wraps IHandleUserDataServices
- [ ] No circular dependencies

---

## Notes

- Sử dụng `class` cho signals (convention của project)
- Interface names follow I-prefix convention
- Constants match exact values từ docs/gameplay-documentation.md
