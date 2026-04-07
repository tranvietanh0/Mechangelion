# Phase 10: Integration

**Effort:** M (2 days)
**Dependencies:** All previous phases
**Blocked By:** Phase 1-9

---

## Objective

Wire everything together: DI registration, full gameplay loop test, polish.

---

## File Ownership

```
Assets/Scripts/Scenes/
├── GameLifetimeScope.cs            [MODIFY]
└── Main/
    └── MainSceneScope.cs           [MODIFY]
```

**Total Files:** 3 (modifications)

---

## Implementation Details

### 1. GameLifetimeScope (Root)

```csharp
// GameLifetimeScope.cs
namespace HyperCasualGame.Scripts.Scenes
{
    using GameFoundationCore.Scripts;
    using GameFoundationCore.Scripts.DI.VContainer;
    using UITemplate.Scripts;
    using VContainer;
    using VContainer.Unity;
    
    // Core
    using HyperCasualGame.Scripts.Core.Signals;
    using HyperCasualGame.Scripts.Infrastructure.Persistence;
    
    // Features - Progression
    using HyperCasualGame.Scripts.Features.Progression.Services;
    
    public class GameLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            // === FRAMEWORK ===
            builder.RegisterGameFoundation(this.transform);
            builder.RegisterUITemplate();
            
            // === INFRASTRUCTURE ===
            builder.Register<SaveService>(Lifetime.Singleton).As<ISaveService>();
            
            // === CORE SIGNALS ===
            // Combat signals
            builder.DeclareSignal<DamageDealtSignal>();
            builder.DeclareSignal<BlockedSignal>();
            builder.DeclareSignal<EntityDefeatedSignal>();
            builder.DeclareSignal<PlayerAttackSignal>();
            
            // Progression signals
            builder.DeclareSignal<LevelUpSignal>();
            builder.DeclareSignal<CurrencyChangedSignal>();
            builder.DeclareSignal<EquipmentUpgradedSignal>();
            
            // Game state signals
            builder.DeclareSignal<BattleStartedSignal>();
            builder.DeclareSignal<BattleEndedSignal>();
            
            // === PROGRESSION (persistent across scenes) ===
            builder.Register<ProgressionService>(Lifetime.Singleton).AsInterfacesAndSelf();
            builder.Register<CurrencyService>(Lifetime.Singleton).AsInterfacesAndSelf();
            builder.Register<EquipmentService>(Lifetime.Singleton).AsInterfacesAndSelf();
            builder.Register<UpgradeService>(Lifetime.Singleton);
        }
    }
}
```

### 2. MainSceneScope (Scene Level)

```csharp
// MainSceneScope.cs
namespace HyperCasualGame.Scripts.Scenes.Main
{
    using System.Linq;
    using GameFoundationCore.Scripts.DI.VContainer;
    using UniT.Extensions;
    using VContainer;
    
    // State Machine
    using HyperCasualGame.Scripts.StateMachines.Game;
    using HyperCasualGame.Scripts.StateMachines.Game.Interfaces;
    
    // Combat
    using HyperCasualGame.Scripts.Features.Combat.Services;
    using HyperCasualGame.Scripts.Features.Combat.Enemy;
    
    // Input
    using HyperCasualGame.Scripts.Features.Input.Services;
    using HyperCasualGame.Scripts.Features.Input.Controllers;
    
    // Animation
    using HyperCasualGame.Scripts.Features.Animation.Services;
    using HyperCasualGame.Scripts.Features.Animation.Effects;
    
    // Battle
    using HyperCasualGame.Scripts.Features.Battle.Services;
    
    public class MainSceneScope : SceneScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            // === GAME STATE MACHINE ===
            builder.Register<GameStateMachine>(Lifetime.Singleton)
                .WithParameter(container => typeof(IGameState)
                    .GetDerivedTypes()
                    .Select(type => (IGameState)container.Instantiate(type))
                    .ToList())
                .AsInterfacesAndSelf();
            
            // === COMBAT SERVICES ===
            builder.Register<DamageCalculator>(Lifetime.Scoped).As<IDamageCalculator>();
            builder.Register<CombatResolver>(Lifetime.Scoped).As<ICombatResolver>();
            builder.Register<CooldownService>(Lifetime.Scoped).AsInterfacesAndSelf();
            
            // === ENEMY ===
            builder.Register<EnemyFactory>(Lifetime.Scoped);
            
            // === INPUT ===
            builder.Register<InputService>(Lifetime.Scoped).AsInterfacesAndSelf();
            builder.Register<CombatInputController>(Lifetime.Scoped).AsInterfacesAndSelf();
            builder.Register<ButtonAvailability>(Lifetime.Scoped);
            
            // === ANIMATION/VFX ===
            builder.Register<TweenService>(Lifetime.Scoped).AsInterfacesAndSelf();
            builder.Register<VFXService>(Lifetime.Scoped).AsInterfacesAndSelf();
            builder.Register<HitEffect>(Lifetime.Scoped);
            builder.Register<SlowMoEffect>(Lifetime.Scoped);
            builder.Register<StatusEffect>(Lifetime.Scoped);
            builder.Register<ScreenShakeEffect>(Lifetime.Scoped);
            
            // === BATTLE ===
            builder.Register<BattleService>(Lifetime.Scoped).AsInterfacesAndSelf();
            builder.Register<RewardService>(Lifetime.Scoped);
            builder.Register<DifficultyService>(Lifetime.Scoped);
        }
    }
}
```

### 3. Gameplay Loop Test

```csharp
// GameplayLoopTest.cs (Editor test or runtime debug)
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class GameplayLoopTest
{
    /// <summary>
    /// Full gameplay loop verification:
    /// 1. Start from Home state
    /// 2. Transition to Battle
    /// 3. Player attacks enemy
    /// 4. Enemy attacks player
    /// 5. Battle ends (victory or defeat)
    /// 6. Rewards applied
    /// 7. Return to Home state
    /// </summary>
    public async UniTask RunFullLoopAsync(
        GameStateMachine stateMachine,
        BattleService battleService,
        PlayerCombatController player,
        ProgressionService progressionService,
        CurrencyService currencyService)
    {
        // Initial state
        int initialCoins = currencyService.GetAmount(CurrencyType.Coins);
        int initialLevel = progressionService.CurrentLevel;
        
        Debug.Log($"[Test] Starting: Level={initialLevel}, Coins={initialCoins}");
        
        // 1. Start battle
        var config = new BattleConfig
        {
            EnemyId = "enemy_robot_01",
            EnemyLevel = initialLevel,
            Difficulty = MissionHardType.Easy
        };
        
        await battleService.StartBattleAsync(config, player);
        Debug.Log("[Test] Battle started");
        
        // 2. Simulate combat (in real game, this is player input)
        // For test, we let AI run or manually trigger actions
        
        // 3. Wait for battle end
        var battleEndTcs = new UniTaskCompletionSource<BattleResult>();
        void OnBattleEnded(BattleResult result) => battleEndTcs.TrySetResult(result);
        battleService.OnBattleEnded += OnBattleEnded;
        
        var result = await battleEndTcs.Task;
        battleService.OnBattleEnded -= OnBattleEnded;
        
        Debug.Log($"[Test] Battle ended: Victory={result.Victory}");
        
        // 4. Verify rewards (if victory)
        if (result.Victory)
        {
            int newCoins = currencyService.GetAmount(CurrencyType.Coins);
            Debug.Assert(newCoins > initialCoins, "Coins should increase after victory");
            Debug.Log($"[Test] Rewards applied: Coins {initialCoins} -> {newCoins}");
        }
        
        // 5. Return to home
        stateMachine.TransitionTo<GameHomeState>();
        Debug.Log("[Test] Returned to home state");
        
        Debug.Log("[Test] Full loop completed successfully!");
    }
}
```

---

## Integration Checklist

### DI Verification
- [ ] All services resolve without errors
- [ ] Lifetime scopes are correct (Singleton vs Scoped)
- [ ] Circular dependencies detected and resolved
- [ ] IInitializable services initialize in correct order

### Signal Flow Verification
- [ ] DamageDealtSignal fires on attack
- [ ] BlockedSignal fires on successful block
- [ ] EntityDefeatedSignal fires on death
- [ ] CurrencyChangedSignal fires on reward
- [ ] LevelUpSignal fires on level up

### State Machine Verification
- [ ] GameHomeState → BattleStartState works
- [ ] BattleStartState → BattleState works
- [ ] BattleState → BattleEndState works
- [ ] BattleEndState → GameHomeState works

### Combat Flow Verification
- [ ] Player attack deals damage to enemy
- [ ] Enemy attack deals damage to player
- [ ] Block reduces damage
- [ ] Dodge avoids damage
- [ ] Cooldowns prevent spam
- [ ] Critical hits apply multiplier

### Progression Verification
- [ ] Victory grants coins
- [ ] Victory grants XP
- [ ] Level up occurs at XP threshold
- [ ] Equipment can be upgraded
- [ ] Data persists across sessions

---

## Known Issues to Fix

| Issue | Priority | Fix |
|-------|----------|-----|
| Missing prefabs | High | Create placeholder prefabs |
| Addressable paths | High | Configure Addressable groups |
| Animation controllers | Medium | Create Animator assets |
| VFX prefabs | Medium | Create particle systems |
| Sound effects | Low | Add AudioService integration |

---

## Final Verification Steps

1. **Clean Build Test**
   ```bash
   # Clear Library folder
   # Open Unity
   # Verify no compile errors
   ```

2. **Play Mode Test**
   - Start from LoadingScene
   - Verify transition to MainScene
   - Start a battle
   - Complete battle (victory)
   - Verify rewards
   - Return to home

3. **Edge Case Tests**
   - Player defeat → revive option
   - Player defeat → give up
   - Enemy with modules → module detachment
   - Low health → slow-mo effect
   - Critical hit → VFX

4. **Performance Check**
   - Profile memory usage
   - Check for GC spikes
   - Verify object pooling works

---

## Post-Integration Tasks

- [ ] Create README for new architecture
- [ ] Document signal flow diagram
- [ ] Add inline code comments where needed
- [ ] Create debug menu for testing
- [ ] Set up automated tests

---

## Commands After Integration

```bash
# Verify compilation
/t1k:test

# Code review
/t1k:review

# Commit
/t1k:git cm "feat: complete Mechangelion rebuild with SOLID architecture"
```
