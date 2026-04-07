# Phase 08: Battle Feature

**Effort:** M (3 days)
**Dependencies:** Phase 4 (Player), Phase 5 (Enemy), Phase 6 (Input)
**Blocked By:** Phase 4, 5, 6

---

## Objective

Implement battle orchestration: service, rewards, difficulty, game states.

---

## File Ownership

```
Assets/Scripts/Features/Battle/
├── Interfaces/
│   └── IBattleResult.cs            [NEW]
│
├── Models/
│   ├── BattleConfig.cs             [NEW]
│   ├── BattleResult.cs             [NEW]
│   └── RewardData.cs               [NEW]
│
├── Services/
│   ├── BattleService.cs            [NEW]
│   ├── RewardService.cs            [NEW]
│   └── DifficultyService.cs        [NEW]
│
└── States/
    ├── BattleState.cs              [NEW]
    ├── BattleStartState.cs         [NEW]
    └── BattleEndState.cs           [NEW]
```

**Total Files:** 10

---

## Implementation Details

### 1. Models

```csharp
// IBattleResult.cs
public interface IBattleResult
{
    bool Victory { get; }
    float Duration { get; }
    int DamageDealt { get; }
    int DamageTaken { get; }
    RewardData Rewards { get; }
}

// BattleConfig.cs
[Serializable]
public class BattleConfig
{
    public string EnemyId { get; set; }
    public int EnemyLevel { get; set; }
    public MissionHardType Difficulty { get; set; }
    public LevelType LevelType { get; set; }
    public string BackgroundId { get; set; }
    
    // Rewards multipliers
    public float CoinMultiplier { get; set; } = 1f;
    public float XPMultiplier { get; set; } = 1f;
}

public enum LevelType
{
    Ordinary,
    PvP,
    Boss,
    CoreBoosted,
    Challenge
}

// BattleResult.cs
public class BattleResult : IBattleResult
{
    public bool Victory { get; set; }
    public float Duration { get; set; }
    public int DamageDealt { get; set; }
    public int DamageTaken { get; set; }
    public RewardData Rewards { get; set; }
    public string DefeatedEnemyId { get; set; }
}

// RewardData.cs
[Serializable]
public class RewardData
{
    public int Coins { get; set; }
    public int Cores { get; set; }
    public int XP { get; set; }
    public List<string> EquipmentDrops { get; set; } = new();
}
```

### 2. Battle Service

```csharp
// BattleService.cs
public class BattleService : IInitializable, IDisposable
{
    private readonly EnemyFactory enemyFactory;
    private readonly ICombatResolver combatResolver;
    private readonly RewardService rewardService;
    private readonly DifficultyService difficultyService;
    private readonly ProgressionService progressionService;
    private readonly SignalBus signalBus;
    
    private BattleConfig currentConfig;
    private EnemyBase currentEnemy;
    private PlayerCombatController player;
    private float battleStartTime;
    private int totalDamageDealt;
    private int totalDamageTaken;
    private bool battleActive;
    
    public bool IsBattleActive => this.battleActive;
    public EnemyBase CurrentEnemy => this.currentEnemy;
    
    public event Action<BattleResult> OnBattleEnded;
    
    public BattleService(
        EnemyFactory enemyFactory,
        ICombatResolver combatResolver,
        RewardService rewardService,
        DifficultyService difficultyService,
        ProgressionService progressionService,
        SignalBus signalBus)
    {
        this.enemyFactory = enemyFactory;
        this.combatResolver = combatResolver;
        this.rewardService = rewardService;
        this.difficultyService = difficultyService;
        this.progressionService = progressionService;
        this.signalBus = signalBus;
    }
    
    public void Initialize()
    {
        this.signalBus.Subscribe<PlayerAttackSignal>(OnPlayerAttack);
        this.signalBus.Subscribe<DamageDealtSignal>(OnDamageDealt);
        this.signalBus.Subscribe<EntityDefeatedSignal>(OnEntityDefeated);
    }
    
    public void Dispose()
    {
        this.signalBus.Unsubscribe<PlayerAttackSignal>(OnPlayerAttack);
        this.signalBus.Unsubscribe<DamageDealtSignal>(OnDamageDealt);
        this.signalBus.Unsubscribe<EntityDefeatedSignal>(OnEntityDefeated);
    }
    
    public async UniTask StartBattleAsync(BattleConfig config, PlayerCombatController player)
    {
        this.currentConfig = config;
        this.player = player;
        this.battleStartTime = Time.time;
        this.totalDamageDealt = 0;
        this.totalDamageTaken = 0;
        
        // Apply difficulty adjustments
        int adjustedLevel = this.difficultyService.AdjustEnemyLevel(
            config.EnemyLevel,
            this.progressionService.CurrentLevel,
            config.Difficulty
        );
        
        // Spawn enemy
        this.currentEnemy = await this.enemyFactory.CreateAsync(config.EnemyId, adjustedLevel);
        
        this.battleActive = true;
        
        this.signalBus.Fire(new BattleStartedSignal
        {
            EnemyId = config.EnemyId,
            Difficulty = config.Difficulty
        });
    }
    
    public void EndBattle(bool victory)
    {
        if (!this.battleActive) return;
        this.battleActive = false;
        
        float duration = Time.time - this.battleStartTime;
        
        // Calculate rewards
        RewardData rewards = null;
        if (victory)
        {
            rewards = this.rewardService.CalculateRewards(
                this.currentConfig,
                this.progressionService.CurrentLevel,
                duration
            );
        }
        
        var result = new BattleResult
        {
            Victory = victory,
            Duration = duration,
            DamageDealt = this.totalDamageDealt,
            DamageTaken = this.totalDamageTaken,
            Rewards = rewards,
            DefeatedEnemyId = victory ? this.currentConfig.EnemyId : null
        };
        
        // Cleanup
        if (this.currentEnemy != null)
        {
            this.enemyFactory.Release(this.currentEnemy);
            this.currentEnemy = null;
        }
        
        this.signalBus.Fire(new BattleEndedSignal { Victory = victory, Duration = duration });
        OnBattleEnded?.Invoke(result);
    }
    
    #region Signal Handlers
    
    private void OnPlayerAttack(PlayerAttackSignal signal)
    {
        if (!this.battleActive || this.currentEnemy == null || this.currentEnemy.IsDead) return;
        
        // Notify enemy AI of incoming attack
        this.currentEnemy.GetComponent<EnemyAI>()?.OnPlayerAttacking();
        
        // Resolve attack
        this.combatResolver.ResolveAttack(this.player, this.currentEnemy, this.player.CurrentWeapon);
    }
    
    private void OnDamageDealt(DamageDealtSignal signal)
    {
        if (!this.battleActive) return;
        
        // Track damage for stats
        if (signal.Target == this.currentEnemy as IDamageable)
        {
            this.totalDamageDealt += (int)signal.Damage.FinalDamage;
        }
        else if (signal.Target == this.player as IDamageable)
        {
            this.totalDamageTaken += (int)signal.Damage.FinalDamage;
        }
    }
    
    private void OnEntityDefeated(EntityDefeatedSignal signal)
    {
        if (!this.battleActive) return;
        
        if (signal.IsPlayer)
        {
            // Player defeated
            EndBattle(false);
        }
        else if (signal.Entity == this.currentEnemy as IDamageable)
        {
            // Enemy defeated
            EndBattle(true);
        }
    }
    
    #endregion
}
```

### 3. Reward Service

```csharp
// RewardService.cs
public class RewardService
{
    private readonly CurrencyService currencyService;
    private readonly ProgressionService progressionService;
    private readonly EquipmentService equipmentService;
    
    private const int BaseCoins = 50;
    private const int BaseXP = 25;
    private const float CoreDropChance = 0.1f;
    private const float EquipmentDropChance = 0.2f;
    
    public RewardService(
        CurrencyService currencyService,
        ProgressionService progressionService,
        EquipmentService equipmentService)
    {
        this.currencyService = currencyService;
        this.progressionService = progressionService;
        this.equipmentService = equipmentService;
    }
    
    public RewardData CalculateRewards(BattleConfig config, int playerLevel, float battleDuration)
    {
        var rewards = new RewardData();
        
        // Base rewards scaled by level
        int levelMultiplier = config.EnemyLevel;
        
        // Coins
        rewards.Coins = (int)(BaseCoins * levelMultiplier * config.CoinMultiplier);
        
        // XP
        rewards.XP = (int)(BaseXP * levelMultiplier * config.XPMultiplier);
        
        // Difficulty bonus
        rewards.Coins = (int)(rewards.Coins * GetDifficultyMultiplier(config.Difficulty));
        rewards.XP = (int)(rewards.XP * GetDifficultyMultiplier(config.Difficulty));
        
        // Core chance
        if (UnityEngine.Random.value < CoreDropChance * (int)config.Difficulty)
        {
            rewards.Cores = 1;
        }
        
        // Equipment drops
        if (UnityEngine.Random.value < EquipmentDropChance)
        {
            string dropId = GenerateEquipmentDrop(config.Difficulty);
            if (!string.IsNullOrEmpty(dropId))
            {
                rewards.EquipmentDrops.Add(dropId);
            }
        }
        
        return rewards;
    }
    
    public void ApplyRewards(RewardData rewards)
    {
        if (rewards == null) return;
        
        this.currencyService.Add(CurrencyType.Coins, rewards.Coins);
        this.currencyService.Add(CurrencyType.Cores, rewards.Cores);
        this.progressionService.AddXP(rewards.XP);
        
        foreach (var equipId in rewards.EquipmentDrops)
        {
            // Create and add equipment
            var equipment = new EquipmentData { Id = equipId, Level = 1, Quantity = 1 };
            this.equipmentService.AddEquipment(equipment);
        }
    }
    
    private float GetDifficultyMultiplier(MissionHardType difficulty)
    {
        return difficulty switch
        {
            MissionHardType.Easy => 1.0f,
            MissionHardType.Medium => 1.5f,
            MissionHardType.Hard => 2.0f,
            _ => 1.0f
        };
    }
    
    private string GenerateEquipmentDrop(MissionHardType difficulty)
    {
        // Implement equipment drop logic based on difficulty
        // Higher difficulty = better equipment chance
        return "weapon_sword_01"; // Placeholder
    }
}
```

### 4. Difficulty Service

```csharp
// DifficultyService.cs
public class DifficultyService
{
    private readonly ProgressionService progressionService;
    
    public DifficultyService(ProgressionService progressionService)
    {
        this.progressionService = progressionService;
    }
    
    public int AdjustEnemyLevel(int baseLevel, int playerLevel, MissionHardType difficulty)
    {
        // Dynamic difficulty adjustment
        float multiplier = GetHardnessMultiplier(difficulty);
        
        // Prevent enemy from being too weak or too strong
        int minLevel = Math.Max(1, playerLevel - 3);
        int maxLevel = playerLevel + 5;
        
        int adjustedLevel = (int)(baseLevel * multiplier);
        return Math.Clamp(adjustedLevel, minLevel, maxLevel);
    }
    
    public float GetEnemyHealthMultiplier(int playerLevel, MissionHardType difficulty)
    {
        // From gameplay docs
        if (playerLevel < 6) return BalanceConstants.EnemyHealthMultiplierLow;
        if (playerLevel < 8) return BalanceConstants.EnemyHealthMultiplierHigh;
        return BalanceConstants.EnemyHealthMultiplierMid;
    }
    
    public float GetHardnessMultiplier(MissionHardType difficulty)
    {
        return difficulty switch
        {
            MissionHardType.Easy => 2.9f,    // 2.7-3.1
            MissionHardType.Medium => 4.2f,  // 4.0-4.4
            MissionHardType.Hard => 5.25f,   // 5.0-5.5
            _ => 3.0f
        };
    }
    
    public bool ShouldReduceDifficulty(float playerDamagePerHit, float enemyHealth)
    {
        float hardnessLimit = playerDamagePerHit * GetHardnessMultiplier(MissionHardType.Easy);
        return enemyHealth > hardnessLimit;
    }
}
```

### 5. Battle States

```csharp
// BattleState.cs
public class BattleState : IGameState, IHaveStateMachine
{
    private readonly BattleService battleService;
    private readonly IScreenManager screenManager;
    
    public IStateMachine StateMachine { get; set; }
    
    public BattleState(BattleService battleService, IScreenManager screenManager)
    {
        this.battleService = battleService;
        this.screenManager = screenManager;
    }
    
    public async void Enter()
    {
        // Subscribe to battle end
        this.battleService.OnBattleEnded += OnBattleEnded;
        
        // Open battle HUD
        await this.screenManager.OpenScreen<BattleHUDPresenter>();
    }
    
    public void Exit()
    {
        this.battleService.OnBattleEnded -= OnBattleEnded;
    }
    
    private void OnBattleEnded(BattleResult result)
    {
        if (result.Victory)
        {
            StateMachine.TransitionTo<BattleEndState>();
        }
        else
        {
            // Show defeat screen or revive option
            StateMachine.TransitionTo<BattleEndState>();
        }
    }
}

// BattleStartState.cs
public class BattleStartState : IGameState, IHaveStateMachine
{
    private readonly BattleService battleService;
    private readonly IGameAssets gameAssets;
    
    public IStateMachine StateMachine { get; set; }
    
    private BattleConfig pendingConfig;
    private PlayerCombatController player;
    
    public BattleStartState(BattleService battleService, IGameAssets gameAssets)
    {
        this.battleService = battleService;
        this.gameAssets = gameAssets;
    }
    
    public void SetConfig(BattleConfig config, PlayerCombatController player)
    {
        this.pendingConfig = config;
        this.player = player;
    }
    
    public async void Enter()
    {
        // Load background
        // Play intro animations
        // Start battle
        await this.battleService.StartBattleAsync(this.pendingConfig, this.player);
        
        // Transition to active battle
        StateMachine.TransitionTo<BattleState>();
    }
    
    public void Exit()
    {
    }
}

// BattleEndState.cs
public class BattleEndState : IGameState, IHaveStateMachine
{
    private readonly BattleService battleService;
    private readonly RewardService rewardService;
    private readonly IScreenManager screenManager;
    
    public IStateMachine StateMachine { get; set; }
    
    public BattleEndState(
        BattleService battleService,
        RewardService rewardService,
        IScreenManager screenManager)
    {
        this.battleService = battleService;
        this.rewardService = rewardService;
        this.screenManager = screenManager;
    }
    
    public async void Enter()
    {
        // Apply rewards if victory
        // Show result screen
        await this.screenManager.OpenScreen<BattleResultPresenter>();
    }
    
    public void Exit()
    {
    }
}
```

---

## DI Registration

```csharp
// MainSceneScope.Configure()
builder.Register<BattleService>(Lifetime.Scoped).AsInterfacesAndSelf();
builder.Register<RewardService>(Lifetime.Scoped);
builder.Register<DifficultyService>(Lifetime.Scoped);
builder.Register<EnemyFactory>(Lifetime.Scoped);

// Battle states auto-register via IGameState reflection
```

---

## Verification Checklist

- [ ] BattleService orchestrates full battle flow
- [ ] RewardService calculates correct rewards
- [ ] DifficultyService adjusts enemy level appropriately
- [ ] States transition correctly: Start → Active → End
- [ ] Signals fire at correct moments
- [ ] Victory/defeat handled properly
