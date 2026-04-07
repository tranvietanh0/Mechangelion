# Phase 03: Combat - Models & Services

**Effort:** S (1 day)
**Dependencies:** Phase 1 (Core interfaces)
**Blocked By:** Phase 1
**Can Run Parallel With:** Phase 2

---

## Objective

Implement combat data models và pure logic services (không có MonoBehaviour).

---

## File Ownership

```
Assets/Scripts/Features/Combat/
├── Interfaces/
│   ├── ICombatant.cs               [NEW]
│   ├── IWeapon.cs                  [NEW]
│   ├── IArmor.cs                   [NEW]
│   ├── IDamageCalculator.cs        [NEW]
│   └── ICombatResolver.cs          [NEW]
│
├── Models/
│   ├── DamageData.cs               [NEW]
│   ├── DamageResult.cs             [NEW]
│   ├── CombatStats.cs              [NEW]
│   └── ActionCooldown.cs           [NEW]
│
└── Services/
    ├── DamageCalculator.cs         [NEW]
    ├── CombatResolver.cs           [NEW]
    └── CooldownService.cs          [NEW]
```

**Total Files:** 12

---

## Implementation Details

### 1. Interfaces

```csharp
// ICombatant.cs
public interface ICombatant : IEntity, IDamageable, IAttacker
{
    CombatStats Stats { get; }
    IWeapon CurrentWeapon { get; }
    IArmor CurrentArmor { get; }
}

// IWeapon.cs
public interface IWeapon
{
    string Id { get; }
    WeaponType Type { get; }
    float BaseDamage { get; }
    float CriticalChance { get; }
    float CriticalMultiplier { get; }
    float ReloadTime { get; }
    float PrepareTime { get; }
}

// IArmor.cs
public interface IArmor
{
    string Id { get; }
    float Defense { get; }
    float CurrentDurability { get; }
    float MaxDurability { get; }
    float GetDamageReduction();
}

// IDamageCalculator.cs
public interface IDamageCalculator
{
    DamageResult Calculate(DamageData data, IDamageable target);
}

// ICombatResolver.cs
public interface ICombatResolver
{
    void ResolveAttack(IAttacker attacker, IDamageable target, IWeapon weapon);
    bool TryBlock(IBlockable blocker, DamageData incomingDamage, out float reducedDamage);
    bool CheckDodge(IDodgeable dodger);
}
```

### 2. Models

```csharp
// DamageData.cs
public readonly struct DamageData
{
    public readonly float BaseDamage;
    public readonly float CriticalChance;
    public readonly float CriticalMultiplier;
    public readonly int AttackerLevel;
    public readonly WeaponType WeaponType;
    public readonly bool IsProjectile;
    
    public DamageData(
        float baseDamage,
        float criticalChance,
        float criticalMultiplier,
        int attackerLevel,
        WeaponType weaponType,
        bool isProjectile = false)
    {
        BaseDamage = baseDamage;
        CriticalChance = criticalChance;
        CriticalMultiplier = criticalMultiplier;
        AttackerLevel = attackerLevel;
        WeaponType = weaponType;
        IsProjectile = isProjectile;
    }
}

// DamageResult.cs
public readonly struct DamageResult
{
    public readonly float RawDamage;
    public readonly float FinalDamage;
    public readonly float DamageBlocked;
    public readonly bool IsCritical;
    public readonly bool WasBlocked;
    public readonly bool WasDodged;
    
    public DamageResult(
        float rawDamage,
        float finalDamage,
        float damageBlocked,
        bool isCritical,
        bool wasBlocked,
        bool wasDodged)
    {
        RawDamage = rawDamage;
        FinalDamage = finalDamage;
        DamageBlocked = damageBlocked;
        IsCritical = isCritical;
        WasBlocked = wasBlocked;
        WasDodged = wasDodged;
    }
    
    public static DamageResult Dodged => new(0, 0, 0, false, false, true);
    public static DamageResult Blocked(float blocked) => new(blocked, 0, blocked, false, true, false);
}

// CombatStats.cs
[Serializable]
public class CombatStats
{
    public float MaxHealth { get; set; } = 100f;
    public float CurrentHealth { get; set; } = 100f;
    public float Attack { get; set; } = 10f;
    public float Defense { get; set; } = 5f;
    public int Level { get; set; } = 1;
    
    public bool IsDead => CurrentHealth <= 0;
    public float HealthPercent => MaxHealth > 0 ? CurrentHealth / MaxHealth : 0;
    
    public void TakeDamage(float amount)
    {
        CurrentHealth = Math.Max(0, CurrentHealth - amount);
    }
    
    public void Heal(float amount)
    {
        CurrentHealth = Math.Min(MaxHealth, CurrentHealth + amount);
    }
    
    public void ResetHealth()
    {
        CurrentHealth = MaxHealth;
    }
}

// ActionCooldown.cs
public class ActionCooldown
{
    public string ActionId { get; }
    public float Duration { get; }
    public float RemainingTime { get; private set; }
    public bool IsReady => RemainingTime <= 0;
    
    public ActionCooldown(string actionId, float duration)
    {
        ActionId = actionId;
        Duration = duration;
        RemainingTime = 0;
    }
    
    public void Trigger()
    {
        RemainingTime = Duration;
    }
    
    public void Update(float deltaTime)
    {
        if (RemainingTime > 0)
            RemainingTime = Math.Max(0, RemainingTime - deltaTime);
    }
    
    public void Reset()
    {
        RemainingTime = 0;
    }
}
```

### 3. Services

```csharp
// DamageCalculator.cs
public class DamageCalculator : IDamageCalculator
{
    private readonly System.Random random = new();
    
    public DamageResult Calculate(DamageData data, IDamageable target)
    {
        // Check critical hit
        bool isCritical = this.random.NextDouble() < data.CriticalChance;
        
        // Calculate raw damage
        float rawDamage = data.BaseDamage;
        if (isCritical)
            rawDamage *= data.CriticalMultiplier;
        
        // Apply level scaling
        rawDamage *= GetLevelMultiplier(data.AttackerLevel);
        
        // Get target's defense (if available via CombatStats)
        float defense = 0f;
        if (target is ICombatant combatant && combatant.CurrentArmor != null)
            defense = combatant.CurrentArmor.GetDamageReduction();
        
        // Calculate final damage
        float finalDamage = Math.Max(0, rawDamage - defense);
        float blocked = rawDamage - finalDamage;
        
        return new DamageResult(rawDamage, finalDamage, blocked, isCritical, false, false);
    }
    
    private float GetLevelMultiplier(int level)
    {
        // Scale damage with level
        return 1f + (level - 1) * 0.1f;
    }
}

// CombatResolver.cs
public class CombatResolver : ICombatResolver
{
    private readonly IDamageCalculator damageCalculator;
    private readonly SignalBus signalBus;
    
    public CombatResolver(IDamageCalculator damageCalculator, SignalBus signalBus)
    {
        this.damageCalculator = damageCalculator;
        this.signalBus = signalBus;
    }
    
    public void ResolveAttack(IAttacker attacker, IDamageable target, IWeapon weapon)
    {
        // Check if target is dodging
        if (target is IDodgeable dodger && this.CheckDodge(dodger))
        {
            this.signalBus.Fire(new DamageDealtSignal { Damage = DamageResult.Dodged });
            return;
        }
        
        // Build damage data
        var damageData = new DamageData(
            weapon.BaseDamage,
            weapon.CriticalChance,
            weapon.CriticalMultiplier,
            attacker is ICombatant c ? c.Stats.Level : 1,
            weapon.Type
        );
        
        // Check if target is blocking
        if (target is IBlockable blocker && this.TryBlock(blocker, damageData, out float reducedDamage))
        {
            var blockedResult = DamageResult.Blocked(reducedDamage);
            this.signalBus.Fire(new BlockedSignal { Blocker = blocker, BlockedAmount = reducedDamage });
            this.signalBus.Fire(new DamageDealtSignal { Damage = blockedResult });
            return;
        }
        
        // Calculate and apply damage
        var result = this.damageCalculator.Calculate(damageData, target);
        target.TakeDamage(result);
        
        this.signalBus.Fire(new DamageDealtSignal { Attacker = attacker, Target = target, Damage = result });
        
        // Check if defeated
        if (target.IsDead)
        {
            this.signalBus.Fire(new EntityDefeatedSignal { Entity = target, IsPlayer = target is IPlayerCombatant });
        }
    }
    
    public bool TryBlock(IBlockable blocker, DamageData incomingDamage, out float reducedDamage)
    {
        reducedDamage = 0;
        if (!blocker.IsBlocking) return false;
        
        reducedDamage = incomingDamage.BaseDamage * blocker.BlockReduction;
        return true;
    }
    
    public bool CheckDodge(IDodgeable dodger)
    {
        return dodger.IsDodging;
    }
}

// CooldownService.cs
public class CooldownService : ITickable
{
    private readonly Dictionary<string, ActionCooldown> cooldowns = new();
    
    public void RegisterCooldown(string actionId, float duration)
    {
        if (!this.cooldowns.ContainsKey(actionId))
            this.cooldowns[actionId] = new ActionCooldown(actionId, duration);
    }
    
    public bool IsReady(string actionId)
    {
        return !this.cooldowns.TryGetValue(actionId, out var cd) || cd.IsReady;
    }
    
    public float GetRemainingTime(string actionId)
    {
        return this.cooldowns.TryGetValue(actionId, out var cd) ? cd.RemainingTime : 0;
    }
    
    public void TriggerCooldown(string actionId)
    {
        if (this.cooldowns.TryGetValue(actionId, out var cd))
            cd.Trigger();
    }
    
    public void ResetCooldown(string actionId)
    {
        if (this.cooldowns.TryGetValue(actionId, out var cd))
            cd.Reset();
    }
    
    public void ResetAll()
    {
        foreach (var cd in this.cooldowns.Values)
            cd.Reset();
    }
    
    // ITickable - called every frame by VContainer
    public void Tick()
    {
        float deltaTime = UnityEngine.Time.deltaTime;
        foreach (var cd in this.cooldowns.Values)
            cd.Update(deltaTime);
    }
}
```

---

## DI Registration (MainSceneScope)

```csharp
// Add to MainSceneScope.Configure()
builder.Register<DamageCalculator>(Lifetime.Scoped).As<IDamageCalculator>();
builder.Register<CombatResolver>(Lifetime.Scoped).As<ICombatResolver>();
builder.Register<CooldownService>(Lifetime.Scoped).AsInterfacesAndSelf();

// Declare signals
builder.DeclareSignal<DamageDealtSignal>();
builder.DeclareSignal<BlockedSignal>();
builder.DeclareSignal<EntityDefeatedSignal>();
```

---

## Verification Checklist

- [ ] DamageCalculator produces correct values per formula
- [ ] CombatResolver fires correct signals
- [ ] CooldownService tracks multiple cooldowns correctly
- [ ] All models are readonly structs or [Serializable] classes
- [ ] No MonoBehaviour dependencies in services

---

## Unit Test Ideas

```csharp
[Test]
public void DamageCalculator_AppliesCriticalCorrectly()
{
    var calc = new DamageCalculator();
    // Mock random to always crit
    // Assert damage *= CriticalMultiplier
}

[Test]
public void CooldownService_TracksMultipleCooldowns()
{
    var service = new CooldownService();
    service.RegisterCooldown("punch", 1f);
    service.RegisterCooldown("block", 4f);
    service.TriggerCooldown("punch");
    Assert.IsFalse(service.IsReady("punch"));
    Assert.IsTrue(service.IsReady("block"));
}
```
