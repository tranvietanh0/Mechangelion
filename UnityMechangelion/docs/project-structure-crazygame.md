# Project Structure - MechangelionCrazyGame

> This document is a legacy/source-project reference for rebuild planning. It does not describe the current runtime structure of `UnityMechangelion`.

Tài liệu này mô tả cấu trúc code của dự án nguồn **MechangelionCrazyGame** để hỗ trợ việc triển khai features trong **UnityMechangelion**.

## Cấu Trúc Thư Mục Tổng Quan

```
Assets/Scripts/
├── AngryGuy/                    # Enemy definitions
├── Player/                      # Player character system
├── Equipment/                   # Weapons & armor
├── GUI/                         # User interface
├── GameState/                   # State management
├── Camera/                      # Camera control
├── AudioFX/                     # Sound system
├── VFX/                         # Visual effects
├── Background/                  # Mission environments
├── Stickman/                    # NPC characters
├── Tween/                       # Animation system
├── JSON_Entities/              # Data serialization
├── BlockingController/         # Global blocking system
├── Addressables/               # Asset loading
└── Editor/                      # Editor tools

TheOneStudio/CrazyGame/        # CrazyGames SDK integration
```

---

## Chi Tiết Từng Module

### 1. AngryGuy/ - Enemy System

```
AngryGuy/
├── Enemy.cs                 # Abstract base class cho tất cả enemies
├── EnemyController.cs       # Spawn/manage current enemy
├── EnemyType.cs            # Enum: ORDINARY, MINION, BOSS, PVP, SMALL
│
├── Angry*.cs               # Individual enemy types
│   ├── AngryRobot1.cs
│   ├── AngryAlienGalaxia.cs
│   ├── AngryDragon.cs
│   ├── AngryBishop.cs
│   └── AngryPlant.cs
│
├── Modules/                # Body part system
│   ├── EnemyModule.cs      # Detachable body parts
│   └── TossedModules.cs    # Projectiles from detached parts
│
├── _Small/                 # Swarm enemies
│   ├── SmallEnemyAbstract.cs
│   ├── SmallEnemiesHerd.cs  # Herd manager
│   └── SmallEnemyParts.cs
│
└── _PvP/                   # PvP-specific
    ├── PvPEnemy.cs
    └── PvPWeapons/
```

**Key Classes:**

| Class | Trách nhiệm |
|-------|-------------|
| `Enemy` | Base class: health, damage, animations, module system |
| `EnemyController` | Singleton quản lý enemy hiện tại, broadcast state changes |
| `EnemyModule` | Body parts có thể tách rời với health riêng |
| `SmallEnemiesHerd` | Quản lý swarm enemies như một nhóm |

---

### 2. Player/ - Player System

```
Player/
├── PlayerController.cs       # Main logic: combat, cooldowns, damage
├── PlayerRobot.cs           # Visual: animations, cosmetics, armor display
├── PlayerArmourComponent.cs # Armor logic
└── PlayerPrefsController.cs # Data persistence (static)
```

**Key Classes:**

| Class | Trách nhiệm |
|-------|-------------|
| `PlayerController` | Combat actions, cooldowns, damage calculation |
| `PlayerRobot` | Animation triggers, cosmetic display, visual states |
| `PlayerArmourComponent` | Armor defense, durability, visual state |
| `PlayerPrefsController` | Static class lưu/đọc tất cả PlayerPrefs |

---

### 3. Equipment/ - Vũ Khí & Giáp

```
Equipment/
├── AbstractEquipment.cs     # Base class cho tất cả equipment
├── AbstractWeapon.cs        # Base class cho weapons
│
├── Melee/
│   ├── MeleeWeapon.cs       # Melee weapon implementation
│   ├── MeleeWeaponType.cs   # Enum: DEFAULT_FIST, GRIP_SWORD, etc.
│   └── MeleeWeaponStats.cs  # Stats data
│
├── Ranged/
│   ├── RangedWeapon.cs      # Ranged weapon implementation
│   ├── WeaponType.cs        # Enum: MELEE, GUN, MACHINEGUN
│   └── EnemyProjectile.cs   # Projectile logic
│
├── Shields/
│   ├── ShieldWeapon.cs      # Shield implementation
│   └── ShieldStats.cs
│
└── Armor/
    ├── ArmorTop.cs
    ├── ArmorBottom.cs
    └── ArmorStats.cs
```

**Equipment Properties:**
```csharp
public class AbstractEquipment
{
    public int damage;
    public float criticalChance;
    public float criticalMultiplier;
    public float reloadTime;
    public int level;
    public int rarity;
    public float defense;  // for armor
}
```

---

### 4. GUI/ - User Interface

```
GUI/
├── ControlButtons/
│   ├── ControlButtonsController.cs  # Input handling
│   ├── ControlButton.cs             # Base button class
│   ├── ControlButtonAttack.cs       # Attack button với prepare
│   └── UIPresentation.cs            # Enum: PUNCH_BLOCK, ALL, etc.
│
├── ProgressBar/
│   ├── HealthBar.cs                 # Health bar UI
│   ├── EnemyHealthBar.cs
│   └── ProgressBarFill.cs
│
├── Screens/
│   ├── MainMenuUI.cs
│   ├── LevelCompleteUI.cs
│   ├── GameOverUI.cs
│   └── PauseUI.cs
│
├── ButtonUpgrade.cs                 # Equipment upgrade button
├── CurrencyDisplay.cs               # Coins/Cores display
└── EquipmentSlotUI.cs               # Equipment slot in inventory
```

**Input Handling Pattern:**
```csharp
// ControlButtonsController checks availability
if (handRightAvailable && commonAvailable)
{
    // Enable attack right button
}

// ControlButtonAttack handles prepare-release pattern
OnPointerDown → Start prepare tween
OnPointerUp → Execute attack if prepared
```

---

### 5. GameState/ - State Management

```
GameState/
├── GameManager.cs           # Main orchestrator
├── GameState.cs             # State pattern (abstract)
├── GameStateType.cs         # Enum: INIT, MENU, GAME_STARTED, ENDGAME
│
├── States/
│   ├── InitState.cs
│   ├── MenuState.cs
│   ├── BattleState.cs
│   └── EndgameState.cs
│
├── Tutorial/
│   ├── TutorialController.cs
│   ├── TutorialStep.cs
│   └── TutorialUI.cs
│
└── LevelType.cs             # Enum: ORDINARY, PVP, BOSS, CORE_BOOSTED, CHALLENGE
```

**State Transition Pattern:**
```csharp
GameManager.SetGameState(GameStateType.GAME_STARTED);
// Triggers:
// 1. currentState.Exit()
// 2. newState.Enter()
// 3. Events broadcast to listeners
```

---

### 6. Tween/ - Animation System

```
Tween/
├── Tween.cs                 # Main tween class
├── TweenController.cs       # Global tween manager (singleton)
├── TweenValue.cs            # Interpolation values
├── Easing.cs                # Easing functions
└── TransformValues.cs       # Position/rotation/scale snapshot
```

**Tween Usage:**
```csharp
TweenController.Instance.CreateTween(
    startValue: 0f,
    endValue: 1f,
    duration: 0.5f,
    easing: Easing.OutQuad,
    onUpdate: (value) => transform.position = Vector3.Lerp(start, end, value),
    onComplete: () => Debug.Log("Done")
);
```

---

### 7. VFX/ - Visual Effects

```
VFX/
├── PunchFX.cs               # Punch impact effects
├── BulletHitFX.cs           # Bullet hit particles
├── SlowMoFX.cs              # Slow motion effect
├── UpgradeFX.cs             # Equipment upgrade particles
├── DamageBoostFX.cs         # Damage boost visual
├── ShieldBoostFX.cs         # Shield boost visual
└── FrostEffectFX.cs         # Freeze status effect
```

---

### 8. AudioFX/ - Sound System

```
AudioFX/
├── AudioController.cs       # Main audio manager
├── SoundType.cs            # Enum cho sound types
├── MusicController.cs       # Background music
└── SFXPool.cs              # Sound effect pooling
```

---

### 9. Background/ - Mission Environments

```
Background/
├── BackgroundController.cs  # Changes background per mission
├── BackgroundType.cs        # Enum cho background types
└── BackgroundPrefabs/       # Prefab references
```

---

### 10. Camera/ - Camera Control

```
Camera/
├── CameraController.cs      # Main camera logic
├── CameraShake.cs           # Screen shake effect
└── CameraTransition.cs      # Battle/menu transitions
```

---

### 11. JSON_Entities/ - Data Serialization

```
JSON_Entities/
├── EquipItem.cs             # Inventory item data
├── RewardsToShow.cs         # Battle rewards data
├── SaveData.cs              # Full save state
└── ConfigData.cs            # Remote config data
```

---

### 12. TheOneStudio/CrazyGame/ - CrazyGames SDK

```
TheOneStudio/CrazyGame/
├── CrazyAdsService.cs       # Ad management
├── CrazyAnalyticService.cs  # Analytics events
├── CrazyPaymentService.cs   # IAP handling
├── CrazyAccountManager.cs   # Cloud save
└── CrazySdkInit.cs          # SDK initialization
```

---

## Class Relationships

### Core Gameplay Loop

```
GameManager (orchestrator)
    │
    ├── GameState (current state)
    │   ├── MenuState
    │   └── BattleState
    │       ├── PlayerController
    │       │   ├── PlayerRobot (visuals)
    │       │   ├── AbstractWeapon (equipped)
    │       │   └── PlayerArmourComponent
    │       │
    │       └── EnemyController
    │           └── Enemy (current enemy)
    │               ├── EnemyModule[] (body parts)
    │               └── SmallEnemiesHerd (if small type)
    │
    └── ControlButtonsController (input)
        └── ControlButton[] (buttons)
```

### Equipment System

```
AbstractEquipment (base)
    │
    ├── AbstractWeapon (weapons)
    │   ├── MeleeWeapon
    │   └── RangedWeapon
    │
    ├── ShieldWeapon
    │
    └── Armor
        ├── ArmorTop
        └── ArmorBottom

EquipItem (inventory data)
    ├── equipmentId
    ├── level
    ├── rarity
    └── specifications
```

### Animation Flow

```
TweenController (singleton manager)
    │
    └── Tween (individual tween)
        ├── TweenValue (start/end/current)
        ├── Easing (interpolation function)
        └── Callbacks
            ├── onStart
            ├── onUpdate(float)
            └── onComplete
```

---

## Patterns Được Sử Dụng

### 1. Singleton Pattern
- `GameManager.Instance`
- `TweenController.Instance`
- `EnemyController.Instance`
- `AudioController.Instance`

### 2. State Pattern
- `GameState` abstract class
- `Enter()` / `Exit()` methods
- State transitions via `GameManager`

### 3. Observer Pattern
- Enemy events broadcast to listeners
- GameState change events
- Health change events

### 4. Object Pooling
- VFX particles pooled
- SFX pooled
- Small enemies reused

### 5. Component Pattern
- `PlayerArmourComponent` attached to player
- `EnemyModule` attached to enemy parts
- Separation of concerns

---

## Data Flow

### Save/Load Flow

```
PlayerPrefsController (static)
    │
    ├── Save: SetInt/SetString → PlayerPrefs → Disk
    │
    └── Load: GetInt/GetString ← PlayerPrefs ← Disk

Keys format: s_id{number}_{suffix}
Example: s_id085_sword01 = quantity of sword01
```

### Remote Config Flow

```
Firebase Remote Config
    │
    └── ConfigData (parsed)
        ├── Enemy multipliers
        ├── Difficulty settings
        └── Balance values
            │
            └── Applied at runtime to:
                ├── Enemy.CalculateDamage()
                └── Enemy.CalculateHealth()
```

---

## Triển Khai Trong UnityMechangelion

### Áp Dụng Patterns

1. **State Machine** → Sử dụng `GameStateMachine` từ UITemplate
2. **DI** → Thay thế singletons bằng VContainer injection
3. **Events** → Thay thế observers bằng SignalBus (MessagePipe)
4. **Async** → Thay thế callbacks bằng UniTask

### Mapping Classes

| CrazyGame | UnityMechangelion |
|-----------|-------------------|
| `GameManager` | `GameLifetimeScope` + `GameStateMachine` |
| Singletons | VContainer registered services |
| `PlayerPrefs` | `IHandleUserDataServices` |
| Callbacks | `UniTask` async/await |
| Events | `SignalBus` signals |

### Addressables

- Enemy prefabs loaded via `IGameAssets.LoadAssetAsync()`
- Background assets via Addressables
- Equipment visuals via Addressables

---

## Related Docs

- [Gameplay Documentation](gameplay-documentation.md) - Chi tiết gameplay
- [System Architecture](system-architecture.md) - Kiến trúc UnityMechangelion
- [Code Standards](code-standards.md) - Coding conventions
