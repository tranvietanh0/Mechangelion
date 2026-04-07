# Gameplay Documentation - Mechangelion Crazy Game

> This document is a legacy/source-game reference for rebuild planning. It does not describe the current runtime implementation of `UnityMechangelion`.

## Tổng Quan Game

**Tên Game:** Mechangelion Crazy Game (CrazyGame trong codebase)

**Thể loại:** Action Fighting / Combat Game với hệ thống Progression

**Core Loop:**
1. Player chiến đấu với enemies được chọn theo level
2. Chiến thắng nhận currency (coins/cores) và equipment
3. Upgrade equipment tăng power
4. Difficulty tăng theo player level
5. Lặp lại

**Các chế độ chơi:**
- Regular Battles (chiến đấu thường)
- PvP (Player vs Player)
- Boss Fights
- Core-boosted Challenges
- Tower Challenges
- Star Quest
- Raid Boss

---

## Hệ Thống Player

### PlayerController - Logic Chính

**Health System:**
- Maximum health với damage reduction (armor-based)
- Armor giảm % damage nhận vào

**Combat Actions:**

| Action | Mô tả | Reload Time |
|--------|-------|-------------|
| Punch (Left/Right) | Melee attack với prepare time | ~1s |
| Block | Defensive ability | 4s |
| Dodge (Left/Right) | Di chuyển né đòn | 1s |
| Fire | Bắn ranged weapon | Tùy weapon |

### Attack Preparation System

```
Button Press → actionPreparing = true → Tween begins
     ↓
Button Release hoặc Max Time reached
     ↓
actionIsPerforming = true → Damage calculation
     ↓
Enemy takes damage → Reload cooldown
```

**Timing Values:**
- Preparation time: 0.75s (45/60 frames)
- Attack execution: 0.167s (10/60 frames)
- Return time: 0.833s (50/60 frames)
- Total cycle: ~1.75s

### Weapon System

**Weapon Types:**

| Type | Mô tả | Key Class |
|------|-------|-----------|
| `MELEE` | Punch-based, swords, axes | `MeleeWeapon` |
| `GUN` | Single shot (plasma, rocket) | `RangedWeapon` |
| `MACHINEGUN` | Automatic fire (minigun) | `RangedWeapon` |

**Melee Weapon Types:**
- `DEFAULT_FIST` - Nắm đấm mặc định
- `GRIP_DEF` - Grip cơ bản
- `GRIP_SWORD` - Kiếm một tay
- `GRIP_SWORD_BIG` - Kiếm hai tay

### Equipment Slots

| Slot | Loại |
|------|------|
| Armor Top | Giáp thân trên |
| Armor Bottom | Giáp thân dưới |
| Left Melee | Vũ khí cận chiến tay trái |
| Right Melee | Vũ khí cận chiến tay phải |
| Ranged | Vũ khí tầm xa |
| Shield | Khiên (cả hai tay) |

### Equipment Properties

```csharp
- Damage rating
- Critical chance & damage multiplier
- Reload times
- Splash radius (ranged)
- Defense values (armor)
- Level (tăng qua upgrade)
- Rarity (common → legendary)
```

---

## Hệ Thống Enemy

### Enemy Types (EnemyType enum)

| Type | Mô tả |
|------|-------|
| `ORDINARY` | Enemy thường với modular components |
| `MINION` | Enemy yếu hơn, support |
| `BOSS` | Boss đặc biệt |
| `PVP` | Player-controlled enemy (PvP mode) |
| `SMALL` | Swarm enemies nhỏ |

### Enemy Architecture

```
Enemy (abstract base)
├── AngryRobot1
├── AngryAlienGalaxia
├── AngryDragon
├── AngryBishop
├── AngryPlant
└── ... (nhiều loại khác)
```

### Module System (Bộ phận tháo rời)

- Enemies có nhiều body parts (modules) có thể tách rời
- Mỗi module có health pool riêng
- Khi module bị phá → bay ra như projectile
- Mất module → giảm stats enemy

**Module Highlight:**
- Modules được highlight dựa trên player focus
- Visual feedback cho player biết đang nhắm vào đâu

### Enemy AI Behaviors

```
Attack Chains:
├── Simple attacks
├── Hard attacks
└── Special attacks

Defensive:
├── Blocking
├── Dodging projectiles
└── Recovery từ knockout

Other:
├── Teasing animations
├── Projectile attacks (blaster, rockets)
└── Knockouts
```

### Small Enemy System

**SmallEnemiesHerd:** Quản lý swarm enemies

| Type | Mô tả |
|------|-------|
| `MELEE` | Tấn công cận chiến |
| `RANGED` | Tấn công từ xa |

- Di chuyển theo tweens
- Attack với cooldown
- Bị phá hủy riêng lẻ

---

## Combat System

### Damage Calculation

```
Final Damage = Base Damage × Level Multiplier × Critical Modifier

Defense Reduction = (Armor Defense × Armor Health %)

Final Damage After Defense = Final Damage - Defense Reduction
```

### Defense System

**Block:**
- Giảm 50-90% damage (tùy enemy type)
- "Super Block" với tight timing window (0.35s)
- Block reload: 4s

**Block Damage Reduction (Robot1):**
- Melee: 50% reduction
- Ranged: 75% reduction khi full health
- Damaged state: 75-90% reduction

### Dodge System

| Direction | Position | Rotation |
|-----------|----------|----------|
| Left | (-2.5, -0.3, 0.1) | (0, 30, 10) |
| Right | (2.5, -0.3, 0.1) | (0, -30, -10) |

### Projectile System

- Enemy fire projectiles (rockets, plasma)
- Tween path từ enemy đến player area
- Fly time: ~0.2s
- Player có thể block hoặc dodge

---

## Progression System

### Level System

**Infinite level cap** - Level không giới hạn

**Enemy scaling theo level:**

| Level Range | Health Multiplier |
|-------------|-------------------|
| < 6 | 0.5x |
| 6-7 | 0.8x |
| 8-9 | 0.75x |

### Difficulty Scaling

**Mission Hardness Types:**

| Type | One-Strike Multiplier |
|------|----------------------|
| EASY | 2.7x - 3.1x |
| MEDIUM | 4.0x - 4.4x |
| HARD | 5.0x - 5.5x |

**Dynamic Difficulty:**
```
If EnemyHealth > PlayerOneStrikeDamage × HardnessMultiplier:
    ReduceLevelBy: (PlayerOneStrikeDamage × Multiplier) / EnemyHealth × original level
```

### Currency System

| Currency | Key | Dùng cho |
|----------|-----|----------|
| Coins | `s_id010` | Equipment upgrades |
| Cores | `s_id011cor` | Special equipment |

### Equipment Upgrade

```
Base cost: 100 coins
Increase every: 3 levels
Maximum cost: 1000 coins
```

**Merge System:** Combine similar items để tăng rarity/level

---

## Game State System

### Game States

```
INIT → IN_MENU → GAME_STARTED → ENDGAME → IN_MENU
```

| State | Mô tả |
|-------|-------|
| `INIT` | Initial loading |
| `IN_MENU` | Main menu |
| `GAME_STARTED` | Active battle |
| `ENDGAME` | Battle finished |

### Battle Flow

**Startup Phase:**
1. Player chọn mission từ menu
2. `GameManager.SetGameState(GAME_STARTED)`
3. `EnemyController.Init()` load enemy từ Addressables
4. Spawn player và enemy
5. Camera transition
6. Enemy intro animation
7. `GameState.TriggerGameplayStart()` (analytics)

**Active Battle:**
```
While Enemy Alive:
    Player Input → Check Availability → Prepare → Execute
    Enemy AI → Choose Action → Execute
    Damage Resolution → VFX → UI Update
    
While Player Alive:
    (tương tự enemy AI)
```

**Victory:**
1. Enemy health = 0
2. Knockout animation
3. Victory FX
4. Rewards calculation:
   - Coins (base + level multiplier)
   - Cores
   - Equipment drops
   - XP
5. Level complete screen
6. Return to menu

**Defeat:**
1. Player health = 0
2. Knockout animation
3. Options:
   - Revive (currency hoặc ad)
   - Give up
4. Process choice → Return to menu

---

## Input System

### Control Buttons

| Button | Class | Function |
|--------|-------|----------|
| Attack Right | `ControlButtonAttack` | Tấn công tay phải |
| Attack Left | `ControlButtonAttack` | Tấn công tay trái |
| Block | `ControlButton` | Chặn đòn |
| Dodge Left | `ControlButton` | Né trái |
| Dodge Right | `ControlButton` | Né phải |

### Button Availability

```csharp
handRightAvailable = handRightAvailable_ && commonAvailable
handLeftAvailable = handLeftAvailable_ && commonAvailable
blockAvailable = blockAvailable_ && commonAvailable
dodgeLeftAvailable = dodgeLeftAvailable_ && commonAvailable
dodgeRightAvailable = dodgeRightAvailable_ && commonAvailable
```

### UI Presentation Modes

| Mode | Hiển thị |
|------|----------|
| `PUNCH_BLOCK` | Punch + Block |
| `PUNCH_FIRE` | Punch + Fire |
| `PUNCH_DODGE` | Punch + Dodge |
| `PUNCH_BLOCK_DODGE` | Punch + Block + Dodge |
| `PUNCH_FIRE_DODGE` | Punch + Fire + Dodge |
| `ALL` | Tất cả buttons |

---

## Animation System

### Tween System (Tween.cs)

**Features:**
- Non-blocking tweens
- Multiple easing functions
- Direction support (FORWARD, BACKWARD)
- Callbacks: `onStart`, `onComplete`, `onUpdate`

**Easing Functions:**
- `InQuad`, `OutQuad`
- `InQuint`, `OutQuint`
- `InOutQuad`, `InOutQuint`
- ... (nhiều hàm khác)

### Animator Parameters

```csharp
// Conditions cho state transitions
idleCondition
punchPrepareCondition, punchAttackCondition
blockCondition, blockedCondition
dodgeLeftCondition, dodgeRightCondition
knockOutCondition
victoryCondition
fireCondition
reviveCondition
```

---

## Visual Effects

### Hit Effects

| Effect | Class | Mô tả |
|--------|-------|-------|
| Punch FX | `PunchFX` | Hiệu ứng đấm |
| Bullet Hit | `BulletHitFX` | Hiệu ứng đạn trúng |
| Slow-Mo | `SlowMoFX` | Hiệu ứng slow motion |

### Status Effects

| Effect | Duration | Mô tả |
|--------|----------|-------|
| Freeze | 2s | Đóng băng |
| Damage Boost | Variable | Tăng damage |
| Shield Boost | Variable | Tăng defense |

---

## Data Persistence (PlayerPrefs)

### Key PlayerPrefs Keys

| Key | Mô tả |
|-----|-------|
| `s_id008` | Level progression |
| `s_id010` | Coins |
| `s_id011cor` | Cores |
| `s_id085_` | Equipment quantities |
| `s_id015_` | Selected equipment |
| `44p5ng_`, `44lt0e_`, `44p5fa_` | Equipment levels/rarity |
| `km119*` | PvP data |
| `t_ply_kid` | Play time |
| `w_rew_kid`, `ct_ad_kid` | Ad watch tracking |
| `c_733d_` | Outfit selections |
| `sid_43505_` | Loot box progression |

---

## External Integrations

### Firebase Remote Config

- Enemy health/damage multipliers
- Difficulty limiters
- Balance adjustments (không cần update code)

### CrazyGames SDK

- Cloud save (CrazyAccountManager)
- Analytics (gameplayStart/gameplayStop)
- HappyTime events

### Monetization

**Ad System:**
- Rewarded ads (revive, rewards)
- Banner ads
- Interstitials
- Moonee SDK (MAX integration)

**IAP:**
- Equipment purchase
- Currency bundles
- No-ads premium

---

## Game Balance Values

### Combat Balance

| Parameter | Value |
|-----------|-------|
| Base punch damage | 0.1 (×100 display) |
| Punch reload | 1s |
| Block reload | 4s |
| Dodge reload | 1s |

### Enemy Balance

| Parameter | Value |
|-----------|-------|
| `BASIC_ENEMY_HEALTH_MULTIPLIER` | 0.5-0.8x (tùy level) |
| `BASIC_ENEMY_DAMAGE_MULTIPLIER` | 0.625x |

---

## Related Docs

- [Project Structure - CrazyGame](project-structure-crazygame.md) - Cấu trúc code source
- [System Architecture](system-architecture.md) - Kiến trúc UnityMechangelion
- [Code Standards](code-standards.md) - Coding conventions
