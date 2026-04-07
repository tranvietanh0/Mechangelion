# Architecture Design - Mechangelion Rebuild

> This document describes target-state rebuild design. It is not the source of truth for the current runtime implementation.
> If an example here conflicts with `docs/code-standards.md`, follow `docs/code-standards.md` for current implementation work.

## Design Decision

**Approach:** Feature-Based Modular Architecture
**Target:** MonoBehaviour + VContainer (không DOTS)
**Goal:** Rebuild từ đầu theo SOLID principles

---

## Proposed Folder Structure

```
Assets/Scripts/
├── Core/                           # Shared abstractions
│   ├── Interfaces/
│   │   ├── IEntity.cs              # Base entity interface
│   │   ├── IDamageable.cs          # Can take damage
│   │   ├── IAttacker.cs            # Can deal damage
│   │   ├── IBlockable.cs           # Can block attacks
│   │   ├── IDodgeable.cs           # Can dodge attacks
│   │   └── IUpgradeable.cs         # Can be upgraded
│   │
│   ├── Data/
│   │   ├── Enums/
│   │   │   ├── WeaponType.cs       # MELEE, GUN, MACHINEGUN
│   │   │   ├── EnemyType.cs        # ORDINARY, BOSS, MINION, PVP, SMALL
│   │   │   ├── EquipmentSlot.cs    # ARMOR_TOP, ARMOR_BOTTOM, MELEE_L, etc.
│   │   │   └── MissionHardType.cs  # EASY, MEDIUM, HARD
│   │   │
│   │   └── Constants/
│   │       ├── CombatConstants.cs  # Timing, multipliers
│   │       └── BalanceConstants.cs # Game balance values
│   │
│   └── Signals/                    # SignalBus event definitions
│       ├── CombatSignals.cs        # DamageDealtSignal, BlockedSignal, etc.
│       ├── ProgressionSignals.cs   # LevelUpSignal, CurrencyChangedSignal
│       └── GameStateSignals.cs     # BattleStartedSignal, BattleEndedSignal
│
├── Features/
│   │
│   ├── Combat/                     # === COMBAT FEATURE ===
│   │   ├── Interfaces/
│   │   │   ├── ICombatant.cs       # Entity that can fight
│   │   │   ├── IWeapon.cs          # Weapon abstraction
│   │   │   ├── IArmor.cs           # Armor abstraction
│   │   │   └── ICombatAction.cs    # Punch, Block, Dodge, Fire
│   │   │
│   │   ├── Models/
│   │   │   ├── DamageData.cs       # Damage calculation data
│   │   │   ├── DamageResult.cs     # Result of damage calculation
│   │   │   ├── CombatStats.cs      # Health, attack, defense stats
│   │   │   └── ActionCooldown.cs   # Cooldown tracking
│   │   │
│   │   ├── Services/
│   │   │   ├── DamageCalculator.cs # Pure damage calculation
│   │   │   ├── CombatResolver.cs   # Resolves combat interactions
│   │   │   └── CooldownService.cs  # Manages action cooldowns
│   │   │
│   │   ├── Player/
│   │   │   ├── PlayerCombatController.cs  # Main player combat logic
│   │   │   ├── PlayerCombatView.cs        # Visual/animation
│   │   │   └── PlayerArmorComponent.cs    # Armor handling
│   │   │
│   │   ├── Enemy/
│   │   │   ├── EnemyController.cs         # Enemy management
│   │   │   ├── EnemyBase.cs               # Base enemy class
│   │   │   ├── EnemyAI.cs                 # AI decision making
│   │   │   ├── EnemyFactory.cs            # Creates enemies
│   │   │   └── Modules/
│   │   │       ├── EnemyModule.cs         # Detachable body part
│   │   │       └── ModuleManager.cs       # Manages modules
│   │   │
│   │   └── Weapons/
│   │       ├── WeaponBase.cs              # Base weapon class
│   │       ├── MeleeWeapon.cs             # Melee implementation
│   │       ├── RangedWeapon.cs            # Ranged implementation
│   │       ├── ShieldWeapon.cs            # Shield implementation
│   │       └── WeaponFactory.cs           # Creates weapons
│   │
│   ├── Progression/                # === PROGRESSION FEATURE ===
│   │   ├── Interfaces/
│   │   │   ├── ILevelable.cs       # Can level up
│   │   │   ├── ICurrency.cs        # Currency abstraction
│   │   │   └── IEquipment.cs       # Equipment abstraction
│   │   │
│   │   ├── Models/
│   │   │   ├── PlayerProgress.cs   # Player level, XP
│   │   │   ├── CurrencyData.cs     # Coins, Cores
│   │   │   ├── EquipmentData.cs    # Equipment stats, level, rarity
│   │   │   └── UpgradeCost.cs      # Upgrade cost calculation
│   │   │
│   │   ├── Services/
│   │   │   ├── ProgressionService.cs    # Level, XP management
│   │   │   ├── CurrencyService.cs       # Currency management
│   │   │   ├── EquipmentService.cs      # Equipment inventory
│   │   │   └── UpgradeService.cs        # Equipment upgrades
│   │   │
│   │   └── Equipment/
│   │       ├── EquipmentManager.cs      # Equipped items
│   │       ├── EquipmentSlot.cs         # Single slot
│   │       └── EquipmentFactory.cs      # Creates equipment
│   │
│   ├── Input/                      # === INPUT FEATURE ===
│   │   ├── Interfaces/
│   │   │   ├── IInputHandler.cs    # Input handling abstraction
│   │   │   └── ICombatInput.cs     # Combat-specific input
│   │   │
│   │   ├── Services/
│   │   │   └── InputService.cs     # Manages input state
│   │   │
│   │   └── Controllers/
│   │       ├── CombatInputController.cs  # Combat input handling
│   │       └── ButtonAvailability.cs     # Button state management
│   │
│   ├── Animation/                  # === ANIMATION FEATURE ===
│   │   ├── Interfaces/
│   │   │   ├── ITweenTarget.cs     # Can be tweened
│   │   │   └── IVFXEmitter.cs      # Can emit VFX
│   │   │
│   │   ├── Services/
│   │   │   ├── TweenService.cs     # Tween management (wraps DOTween or custom)
│   │   │   └── VFXService.cs       # VFX pooling and emission
│   │   │
│   │   └── Effects/
│   │       ├── HitEffect.cs        # Punch/hit VFX
│   │       ├── SlowMoEffect.cs     # Slow motion effect
│   │       └── StatusEffect.cs     # Freeze, boost VFX
│   │
│   └── Battle/                     # === BATTLE FEATURE ===
│       ├── Interfaces/
│       │   └── IBattleResult.cs    # Battle outcome
│       │
│       ├── Models/
│       │   ├── BattleConfig.cs     # Mission config
│       │   ├── BattleResult.cs     # Victory/defeat data
│       │   └── RewardData.cs       # Rewards calculation
│       │
│       ├── Services/
│       │   ├── BattleService.cs    # Battle orchestration
│       │   ├── RewardService.cs    # Calculates rewards
│       │   └── DifficultyService.cs # Dynamic difficulty
│       │
│       └── States/
│           ├── BattleState.cs      # IGameState for battle
│           ├── BattleStartState.cs # Setup phase
│           ├── BattleActiveState.cs # Active combat
│           └── BattleEndState.cs   # Victory/defeat
│
├── Infrastructure/                 # External dependencies
│   ├── Persistence/
│   │   ├── ISaveService.cs         # Save/load abstraction
│   │   └── SaveService.cs          # Wraps IHandleUserDataServices
│   │
│   └── Assets/
│       └── AddressableLoader.cs    # Wraps IGameAssets
│
├── UI/                             # UI Screens (MVP pattern)
│   ├── Battle/
│   │   ├── BattleHUDView.cs        # Health bars, controls
│   │   ├── BattleHUDPresenter.cs
│   │   └── ControlButtonView.cs    # Combat buttons
│   │
│   ├── Menu/
│   │   ├── HomeScreenView.cs
│   │   ├── HomeScreenPresenter.cs
│   │   ├── EquipmentScreenView.cs
│   │   └── EquipmentScreenPresenter.cs
│   │
│   └── Shared/
│       ├── HealthBarView.cs
│       └── CurrencyDisplayView.cs
│
├── Models/                         # Existing folder
│   └── UserLocalData.cs            # Keep existing
│
└── Scenes/                         # DI Scopes
    ├── GameLifetimeScope.cs        # Root: Core + Infrastructure
    ├── Loading/LoadingSceneScope.cs
    └── Main/
        ├── MainSceneScope.cs       # Features registration
        └── BattleSceneScope.cs     # Battle-specific (if separate scene)
```

---

## SOLID Mapping

### S - Single Responsibility

| Class | Single Responsibility |
|-------|----------------------|
| `DamageCalculator` | Chỉ tính damage |
| `CooldownService` | Chỉ quản lý cooldowns |
| `PlayerCombatController` | Chỉ xử lý combat actions |
| `PlayerCombatView` | Chỉ xử lý visual/animation |
| `EnemyAI` | Chỉ quyết định AI actions |
| `EquipmentService` | Chỉ quản lý inventory |
| `UpgradeService` | Chỉ xử lý upgrades |

### O - Open/Closed

```csharp
// Base weapon - closed for modification
public abstract class WeaponBase : IWeapon
{
    public abstract DamageData CalculateDamage();
}

// Extensions - open for extension
public class MeleeWeapon : WeaponBase { ... }
public class RangedWeapon : WeaponBase { ... }
public class ShieldWeapon : WeaponBase { ... }
```

### L - Liskov Substitution

```csharp
// Any IWeapon can be used interchangeably
void Attack(IWeapon weapon)
{
    var damage = weapon.CalculateDamage();
    // Works for MeleeWeapon, RangedWeapon, etc.
}
```

### I - Interface Segregation

```csharp
// Small, focused interfaces
public interface IDamageable
{
    void TakeDamage(DamageResult damage);
    float CurrentHealth { get; }
}

public interface IBlockable
{
    bool IsBlocking { get; }
    float BlockReduction { get; }
}

public interface IDodgeable
{
    bool IsDodging { get; }
    void Dodge(DodgeDirection direction);
}

// Entity implements only what it needs
public class PlayerCombatController : IDamageable, IBlockable, IDodgeable { }
public class SmallEnemy : IDamageable { } // Can't block or dodge
```

### D - Dependency Inversion

```csharp
// High-level depends on abstraction
public class BattleService
{
    private readonly IDamageCalculator _damageCalculator;
    private readonly ICombatResolver _combatResolver;
    
    public BattleService(
        IDamageCalculator damageCalculator,
        ICombatResolver combatResolver)
    {
        _damageCalculator = damageCalculator;
        _combatResolver = combatResolver;
    }
}

// Registered in VContainer
builder.Register<DamageCalculator>(Lifetime.Singleton).As<IDamageCalculator>();
```

---

## VContainer Registration Pattern

### GameLifetimeScope (Root)

```csharp
protected override void Configure(IContainerBuilder builder)
{
    builder.RegisterGameFoundation(this.transform);
    builder.RegisterUITemplate();
    
    // Core services (singleton, shared across scenes)
    builder.Register<SaveService>(Lifetime.Singleton).As<ISaveService>();
    
    // Progression (persistent)
    builder.Register<ProgressionService>(Lifetime.Singleton).AsInterfacesAndSelf();
    builder.Register<CurrencyService>(Lifetime.Singleton).AsInterfacesAndSelf();
    builder.Register<EquipmentService>(Lifetime.Singleton).AsInterfacesAndSelf();
}
```

### MainSceneScope (Scene Level)

```csharp
protected override void Configure(IContainerBuilder builder)
{
    // Game state machine
    builder.Register<GameStateMachine>(Lifetime.Singleton)
        .WithParameter(/* auto-register states */)
        .AsInterfacesAndSelf();
    
    // Combat services (per-battle lifecycle)
    builder.Register<DamageCalculator>(Lifetime.Scoped).As<IDamageCalculator>();
    builder.Register<CombatResolver>(Lifetime.Scoped).As<ICombatResolver>();
    builder.Register<CooldownService>(Lifetime.Scoped);
    
    // Battle services
    builder.Register<BattleService>(Lifetime.Scoped);
    builder.Register<RewardService>(Lifetime.Scoped);
    builder.Register<DifficultyService>(Lifetime.Scoped);
    
    // Animation services
    builder.Register<TweenService>(Lifetime.Scoped);
    builder.Register<VFXService>(Lifetime.Scoped);
    
    // Input
    builder.Register<InputService>(Lifetime.Scoped);
    builder.Register<CombatInputController>(Lifetime.Scoped);
}
```

---

## Signal Definitions

```csharp
// CombatSignals.cs
public class DamageDealtSignal
{
    public ICombatant Attacker;
    public ICombatant Target;
    public DamageResult Damage;
}

public class BlockedSignal
{
    public ICombatant Blocker;
    public DamageData BlockedDamage;
}

public class EnemyDefeatedSignal
{
    public EnemyBase Enemy;
}

public class PlayerDefeatedSignal { }

// ProgressionSignals.cs
public class LevelUpSignal
{
    public int NewLevel;
}

public class CurrencyChangedSignal
{
    public CurrencyType Type;
    public int OldAmount;
    public int NewAmount;
}

public class EquipmentUpgradedSignal
{
    public EquipmentData Equipment;
    public int NewLevel;
}
```

---

## Implementation Phases

### Phase 1: Core + Infrastructure
- Create interfaces (IEntity, IDamageable, IAttacker, etc.)
- Create enums (WeaponType, EnemyType, etc.)
- Create constants (CombatConstants, BalanceConstants)
- Create signals (CombatSignals, ProgressionSignals)
- Create SaveService wrapper

### Phase 2: Progression Feature
- ProgressionService (level, XP)
- CurrencyService (coins, cores)
- EquipmentData models
- EquipmentService (inventory)
- UpgradeService

### Phase 3: Combat Feature - Models & Services
- DamageData, DamageResult, CombatStats models
- DamageCalculator
- CombatResolver
- CooldownService

### Phase 4: Combat Feature - Player
- PlayerCombatController
- PlayerCombatView
- PlayerArmorComponent
- Weapon implementations (MeleeWeapon, RangedWeapon, ShieldWeapon)

### Phase 5: Combat Feature - Enemy
- EnemyBase
- EnemyController
- EnemyAI
- EnemyModule system
- EnemyFactory

### Phase 6: Input Feature
- InputService
- CombatInputController
- ButtonAvailability

### Phase 7: Animation/VFX Feature
- TweenService
- VFXService
- Effect implementations

### Phase 8: Battle Feature
- BattleService
- RewardService
- DifficultyService
- Battle states (Start, Active, End)

### Phase 9: UI
- BattleHUDView/Presenter
- ControlButtonView
- HealthBarView

### Phase 10: Integration
- Wire everything in DI scopes
- Full gameplay loop test
- Polish and bug fixes

---

## Key Design Decisions

### 1. Service vs Controller
- **Service**: Pure logic, no Unity dependencies (testable)
- **Controller**: MonoBehaviour, handles Unity lifecycle

### 2. Factory Pattern for Enemies/Weapons
- Decouple creation from usage
- Easy to add new types without modifying existing code

### 3. Signals for Cross-Feature Communication
- Combat → Progression: `EnemyDefeatedSignal` triggers XP/currency
- Progression → UI: `CurrencyChangedSignal` updates display
- No direct coupling between features

### 4. Cooldown as Service
- Centralized cooldown tracking
- Testable without MonoBehaviour
- Combat controller just asks "can I do action X?"

### 5. Tween Abstraction
- `TweenService` wraps actual implementation (DOTween or custom)
- Can swap tween library without touching game code

---

## Dependencies Between Features

```
                    ┌─────────────┐
                    │    Core     │  (Interfaces, Enums, Signals)
                    └──────┬──────┘
                           │
         ┌─────────────────┼─────────────────┐
         │                 │                 │
         ▼                 ▼                 ▼
   ┌───────────┐    ┌───────────┐    ┌───────────┐
   │Progression│    │  Combat   │    │ Animation │
   └─────┬─────┘    └─────┬─────┘    └─────┬─────┘
         │                │                 │
         │    ┌───────────┴───────┐         │
         │    │                   │         │
         │    ▼                   ▼         │
         │  ┌─────┐          ┌────────┐     │
         │  │Input│          │ Battle │◄────┘
         │  └──┬──┘          └────┬───┘
         │     │                  │
         │     └────────┬─────────┘
         │              │
         └──────────────┼──────────────────────┐
                        ▼                      │
                  ┌──────────┐                 │
                  │    UI    │◄────────────────┘
                  └──────────┘
```

---

## Related Docs

- [Gameplay Documentation](gameplay-documentation.md) - Chi tiết gameplay gốc
- [Project Structure - CrazyGame](project-structure-crazygame.md) - Cấu trúc code source
- [System Architecture](system-architecture.md) - Kiến trúc hiện tại
