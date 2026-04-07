# Phase 05: Combat - Enemy

**Effort:** L (4 days)
**Dependencies:** Phase 3 (Combat services)
**Blocked By:** Phase 3
**Can Run Parallel With:** Phase 4

---

## Objective

Implement enemy system: base class, AI, modules, factory.

---

## File Ownership

```
Assets/Scripts/Features/Combat/Enemy/
├── EnemyBase.cs                    [NEW]
├── EnemyController.cs              [NEW]
├── EnemyAI.cs                      [NEW]
├── EnemyFactory.cs                 [NEW]
├── EnemyConfig.cs                  [NEW]
│
└── Modules/
    ├── EnemyModule.cs              [NEW]
    ├── ModuleManager.cs            [NEW]
    ├── ModuleHealth.cs             [NEW]
    └── TossedModule.cs             [NEW]
```

**Total Files:** 10

---

## Implementation Details

### 1. Enemy Base

```csharp
// EnemyBase.cs
public abstract class EnemyBase : MonoBehaviour, ICombatant, IBlockable
{
    #region Serialized
    
    [SerializeField] protected EnemyConfig config;
    [SerializeField] protected Animator animator;
    [SerializeField] protected ModuleManager moduleManager;
    
    #endregion
    
    #region State
    
    protected CombatStats stats;
    protected bool isBlocking;
    protected bool isAttacking;
    protected bool isStunned;
    
    #endregion
    
    #region Properties
    
    public string Id => this.config.Id;
    public EnemyType Type => this.config.Type;
    public bool IsActive => !this.stats.IsDead && gameObject.activeInHierarchy;
    
    public CombatStats Stats => this.stats;
    public IWeapon CurrentWeapon => null; // Enemies use built-in attacks
    public IArmor CurrentArmor => null;
    
    public float CurrentHealth => this.stats.CurrentHealth;
    public float MaxHealth => this.stats.MaxHealth;
    public bool IsDead => this.stats.IsDead;
    
    public bool IsBlocking => this.isBlocking;
    public float BlockReduction => this.config.BlockReduction;
    
    public bool CanAttack => !this.isAttacking && !this.isStunned && !this.isBlocking && !IsDead;
    
    #endregion
    
    #region Events
    
    public event Action<DamageResult> OnDamageTaken;
    public event Action OnDefeated;
    
    #endregion
    
    #region Initialization
    
    public virtual void Initialize(int level)
    {
        this.stats = new CombatStats
        {
            MaxHealth = this.config.BaseHealth * GetHealthMultiplier(level),
            Attack = this.config.BaseAttack * GetDamageMultiplier(level),
            Defense = this.config.BaseDefense,
            Level = level
        };
        this.stats.ResetHealth();
        
        this.moduleManager?.Initialize(this);
    }
    
    protected virtual float GetHealthMultiplier(int level)
    {
        if (level < 6) return BalanceConstants.EnemyHealthMultiplierLow;
        if (level < 8) return BalanceConstants.EnemyHealthMultiplierHigh;
        return BalanceConstants.EnemyHealthMultiplierMid;
    }
    
    protected virtual float GetDamageMultiplier(int level)
    {
        return BalanceConstants.EnemyDamageMultiplier;
    }
    
    #endregion
    
    #region Combat
    
    public DamageData GetBaseDamage()
    {
        return new DamageData(
            this.stats.Attack,
            this.config.CriticalChance,
            this.config.CriticalMultiplier,
            this.stats.Level,
            WeaponType.Melee
        );
    }
    
    public void TakeDamage(DamageResult damage)
    {
        if (damage.WasDodged) return;
        
        this.stats.TakeDamage(damage.FinalDamage);
        OnDamageTaken?.Invoke(damage);
        
        // Apply damage to modules if hit specific area
        this.moduleManager?.TakeDamage(damage);
        
        // Play animation
        PlayHitAnimation(damage);
        
        if (this.stats.IsDead)
        {
            OnDefeated?.Invoke();
            PlayDeathAnimation();
        }
    }
    
    public bool TryBlock()
    {
        if (this.isBlocking || this.isAttacking || this.isStunned) return false;
        
        this.isBlocking = true;
        PlayBlockAnimation();
        return true;
    }
    
    public void EndBlock()
    {
        this.isBlocking = false;
    }
    
    #endregion
    
    #region Attack Methods
    
    public virtual void PerformSimpleAttack()
    {
        if (!CanAttack) return;
        this.isAttacking = true;
        PlayAttackAnimation(EnemyAttackType.Simple);
    }
    
    public virtual void PerformHardAttack()
    {
        if (!CanAttack) return;
        this.isAttacking = true;
        PlayAttackAnimation(EnemyAttackType.Hard);
    }
    
    public virtual void PerformSpecialAttack()
    {
        if (!CanAttack) return;
        this.isAttacking = true;
        PlayAttackAnimation(EnemyAttackType.Special);
    }
    
    // Called by animation events
    public void OnAttackComplete()
    {
        this.isAttacking = false;
    }
    
    #endregion
    
    #region Animation (virtual for override)
    
    protected virtual void PlayHitAnimation(DamageResult damage)
    {
        this.animator?.SetTrigger("Hit");
    }
    
    protected virtual void PlayDeathAnimation()
    {
        this.animator?.SetTrigger("Death");
    }
    
    protected virtual void PlayBlockAnimation()
    {
        this.animator?.SetTrigger("Block");
    }
    
    protected virtual void PlayAttackAnimation(EnemyAttackType type)
    {
        this.animator?.SetTrigger(type.ToString());
    }
    
    #endregion
}

public enum EnemyAttackType
{
    Simple,
    Hard,
    Special
}
```

### 2. Enemy Config

```csharp
// EnemyConfig.cs
[CreateAssetMenu(fileName = "EnemyConfig", menuName = "Mechangelion/Enemy Config")]
public class EnemyConfig : ScriptableObject
{
    [Header("Identity")]
    public string Id;
    public EnemyType Type;
    public string DisplayName;
    
    [Header("Base Stats")]
    public float BaseHealth = 100f;
    public float BaseAttack = 10f;
    public float BaseDefense = 5f;
    
    [Header("Combat")]
    public float BlockReduction = 0.5f;
    public float CriticalChance = 0.1f;
    public float CriticalMultiplier = 1.5f;
    
    [Header("AI")]
    public float AttackCooldown = 2f;
    public float BlockChance = 0.3f;
    public float SpecialAttackChance = 0.1f;
    
    [Header("Modules")]
    public bool HasModules = true;
    public int ModuleCount = 4;
}
```

### 3. Enemy AI

```csharp
// EnemyAI.cs
public class EnemyAI : MonoBehaviour
{
    [SerializeField] private EnemyBase enemy;
    [SerializeField] private float decisionInterval = 0.5f;
    
    private float lastDecisionTime;
    private float attackCooldownRemaining;
    private System.Random random = new();
    
    private EnemyConfig Config => this.enemy.config;
    
    public void Initialize()
    {
        this.lastDecisionTime = 0f;
        this.attackCooldownRemaining = 0f;
    }
    
    private void Update()
    {
        if (this.enemy.IsDead || !this.enemy.IsActive) return;
        
        this.attackCooldownRemaining -= Time.deltaTime;
        
        if (Time.time - this.lastDecisionTime >= this.decisionInterval)
        {
            MakeDecision();
            this.lastDecisionTime = Time.time;
        }
    }
    
    private void MakeDecision()
    {
        if (!this.enemy.CanAttack) return;
        
        // Check if should block (reactive - based on player state)
        // This would be informed by signals from player
        
        // Check attack cooldown
        if (this.attackCooldownRemaining > 0) return;
        
        // Decide attack type
        float roll = (float)this.random.NextDouble();
        
        if (roll < Config.SpecialAttackChance)
        {
            this.enemy.PerformSpecialAttack();
        }
        else if (roll < Config.SpecialAttackChance + 0.3f)
        {
            this.enemy.PerformHardAttack();
        }
        else
        {
            this.enemy.PerformSimpleAttack();
        }
        
        this.attackCooldownRemaining = Config.AttackCooldown;
    }
    
    // Called by signals when player is about to attack
    public void OnPlayerAttacking()
    {
        if (this.enemy.IsDead || this.enemy.IsBlocking) return;
        
        float roll = (float)this.random.NextDouble();
        if (roll < Config.BlockChance)
        {
            this.enemy.TryBlock();
        }
    }
}
```

### 4. Module System

```csharp
// EnemyModule.cs
public class EnemyModule : MonoBehaviour
{
    [SerializeField] private string moduleId;
    [SerializeField] private float maxHealth = 50f;
    [SerializeField] private Renderer renderer;
    [SerializeField] private Collider collider;
    
    private float currentHealth;
    private bool isDetached;
    
    public string ModuleId => this.moduleId;
    public bool IsDetached => this.isDetached;
    public float HealthPercent => this.maxHealth > 0 ? this.currentHealth / this.maxHealth : 0;
    
    public event Action<EnemyModule> OnDetached;
    
    public void Initialize()
    {
        this.currentHealth = this.maxHealth;
        this.isDetached = false;
    }
    
    public void TakeDamage(float amount)
    {
        if (this.isDetached) return;
        
        this.currentHealth -= amount;
        UpdateVisual();
        
        if (this.currentHealth <= 0)
        {
            Detach();
        }
    }
    
    public void Highlight(bool enabled)
    {
        // Change material or outline
        if (this.renderer != null)
        {
            // Apply highlight shader or color
        }
    }
    
    private void Detach()
    {
        this.isDetached = true;
        this.collider.enabled = false;
        OnDetached?.Invoke(this);
        
        // Spawn tossed module
        SpawnTossedModule();
    }
    
    private void SpawnTossedModule()
    {
        // Instantiate TossedModule prefab at this position
        // Apply physics force
    }
    
    private void UpdateVisual()
    {
        // Update damage state visual (cracks, sparks, etc.)
    }
}

// ModuleManager.cs
public class ModuleManager : MonoBehaviour
{
    [SerializeField] private List<EnemyModule> modules = new();
    
    private EnemyBase enemy;
    
    public IReadOnlyList<EnemyModule> Modules => this.modules;
    public int ActiveModuleCount => this.modules.Count(m => !m.IsDetached);
    
    public event Action<EnemyModule> OnModuleDetached;
    
    public void Initialize(EnemyBase enemy)
    {
        this.enemy = enemy;
        foreach (var module in this.modules)
        {
            module.Initialize();
            module.OnDetached += HandleModuleDetached;
        }
    }
    
    public void TakeDamage(DamageResult damage)
    {
        // Distribute damage to random module or based on hit location
        var activeModules = this.modules.Where(m => !m.IsDetached).ToList();
        if (activeModules.Count == 0) return;
        
        var targetModule = activeModules[UnityEngine.Random.Range(0, activeModules.Count)];
        targetModule.TakeDamage(damage.FinalDamage * 0.3f); // 30% damage to module
    }
    
    public void HighlightModule(string moduleId, bool enabled)
    {
        var module = this.modules.FirstOrDefault(m => m.ModuleId == moduleId);
        module?.Highlight(enabled);
    }
    
    private void HandleModuleDetached(EnemyModule module)
    {
        OnModuleDetached?.Invoke(module);
        
        // Reduce enemy stats when modules are lost
        this.enemy.Stats.Attack *= 0.9f; // -10% attack per module
    }
    
    private void OnDestroy()
    {
        foreach (var module in this.modules)
        {
            module.OnDetached -= HandleModuleDetached;
        }
    }
}

// TossedModule.cs
public class TossedModule : MonoBehaviour
{
    [SerializeField] private Rigidbody rigidbody;
    [SerializeField] private float lifetime = 5f;
    
    public void Launch(Vector3 force)
    {
        this.rigidbody.AddForce(force, ForceMode.Impulse);
        this.rigidbody.AddTorque(UnityEngine.Random.insideUnitSphere * 10f, ForceMode.Impulse);
        
        Destroy(gameObject, this.lifetime);
    }
}
```

### 5. Enemy Factory

```csharp
// EnemyFactory.cs
public class EnemyFactory
{
    private readonly IGameAssets gameAssets;
    private readonly Dictionary<string, EnemyConfig> configCache = new();
    
    public EnemyFactory(IGameAssets gameAssets)
    {
        this.gameAssets = gameAssets;
    }
    
    public async UniTask<EnemyBase> CreateAsync(string enemyId, int level, Transform parent = null)
    {
        // Load enemy prefab via Addressables
        var prefab = await this.gameAssets.LoadAssetAsync<GameObject>($"Enemies/{enemyId}");
        
        var instance = UnityEngine.Object.Instantiate(prefab, parent);
        var enemy = instance.GetComponent<EnemyBase>();
        
        if (enemy == null)
        {
            UnityEngine.Object.Destroy(instance);
            throw new InvalidOperationException($"Enemy prefab {enemyId} missing EnemyBase component");
        }
        
        enemy.Initialize(level);
        return enemy;
    }
    
    public void Release(EnemyBase enemy)
    {
        if (enemy != null)
        {
            UnityEngine.Object.Destroy(enemy.gameObject);
        }
    }
}
```

---

## Verification Checklist

- [ ] EnemyBase implements all required interfaces
- [ ] EnemyConfig ScriptableObject creates correctly
- [ ] EnemyAI makes decisions at correct intervals
- [ ] Module system detaches parts correctly
- [ ] EnemyFactory loads from Addressables
- [ ] Stats scale with level per BalanceConstants

---

## Notes

- Concrete enemy types (AngryRobot1, etc.) extend EnemyBase
- EnemyConfig is ScriptableObject for easy balancing
- Module damage is 30% of main damage (adjustable)
- AI decision interval prevents spam
