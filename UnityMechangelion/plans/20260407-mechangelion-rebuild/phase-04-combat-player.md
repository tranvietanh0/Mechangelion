# Phase 04: Combat - Player

**Effort:** M (3 days)
**Dependencies:** Phase 2 (Progression - equipment), Phase 3 (Combat services)
**Blocked By:** Phase 2, Phase 3

---

## Objective

Implement player combat: controller, view, armor, weapons.

---

## File Ownership

```
Assets/Scripts/Features/Combat/
├── Player/
│   ├── PlayerCombatController.cs   [NEW]
│   ├── PlayerCombatView.cs         [NEW]
│   └── PlayerArmorComponent.cs     [NEW]
│
└── Weapons/
    ├── WeaponBase.cs               [NEW]
    ├── MeleeWeapon.cs              [NEW]
    ├── RangedWeapon.cs             [NEW]
    ├── ShieldWeapon.cs             [NEW]
    └── WeaponFactory.cs            [NEW]
```

**Total Files:** 8

---

## Implementation Details

### 1. Player Combat Controller

```csharp
// PlayerCombatController.cs
public class PlayerCombatController : MonoBehaviour, ICombatant, IBlockable, IDodgeable
{
    #region Dependencies (method injection for MonoBehaviour)
    
    private ICombatResolver combatResolver;
    private CooldownService cooldownService;
    private EquipmentService equipmentService;
    private SignalBus signalBus;
    
    [Inject]
    public void Construct(
        ICombatResolver combatResolver,
        CooldownService cooldownService,
        EquipmentService equipmentService,
        SignalBus signalBus)
    {
        this.combatResolver = combatResolver;
        this.cooldownService = cooldownService;
        this.equipmentService = equipmentService;
        this.signalBus = signalBus;
    }
    
    #endregion
    
    #region Serialized Fields
    
    [SerializeField] private PlayerCombatView view;
    
    #endregion
    
    #region State
    
    private CombatStats stats = new();
    private bool isBlocking;
    private bool isDodging;
    private DodgeDirection dodgeDirection;
    private bool isPreparing;
    private float prepareProgress;
    
    #endregion
    
    #region ICombatant Implementation
    
    public string Id => "player";
    public bool IsActive => !this.stats.IsDead;
    public CombatStats Stats => this.stats;
    public IWeapon CurrentWeapon => this.GetEquippedWeapon();
    public IArmor CurrentArmor => this.GetEquippedArmor();
    
    public float CurrentHealth => this.stats.CurrentHealth;
    public float MaxHealth => this.stats.MaxHealth;
    public bool IsDead => this.stats.IsDead;
    
    public DamageData GetBaseDamage()
    {
        var weapon = this.CurrentWeapon;
        return new DamageData(
            weapon?.BaseDamage ?? CombatConstants.BaseDamage,
            weapon?.CriticalChance ?? 0.1f,
            weapon?.CriticalMultiplier ?? 1.5f,
            this.stats.Level,
            weapon?.Type ?? WeaponType.Melee
        );
    }
    
    public bool CanAttack => !this.isPreparing && !this.isBlocking && !this.isDodging 
        && this.cooldownService.IsReady("punch_right") || this.cooldownService.IsReady("punch_left");
    
    public void TakeDamage(DamageResult damage)
    {
        if (damage.WasDodged) return;
        
        this.stats.TakeDamage(damage.FinalDamage);
        this.view.PlayDamageAnimation(damage);
        
        if (this.stats.IsDead)
        {
            this.view.PlayDeathAnimation();
        }
    }
    
    public event Action<DamageResult> OnDamageTaken;
    
    #endregion
    
    #region IBlockable Implementation
    
    public bool IsBlocking => this.isBlocking;
    public float BlockReduction => 0.5f; // 50% reduction
    
    public bool TryBlock()
    {
        if (!this.cooldownService.IsReady("block") || this.isPreparing || this.isDodging)
            return false;
        
        this.isBlocking = true;
        this.cooldownService.TriggerCooldown("block");
        this.view.PlayBlockAnimation();
        return true;
    }
    
    #endregion
    
    #region IDodgeable Implementation
    
    public bool IsDodging => this.isDodging;
    
    public void Dodge(DodgeDirection direction)
    {
        string cooldownId = direction == DodgeDirection.Left ? "dodge_left" : "dodge_right";
        if (!this.cooldownService.IsReady(cooldownId) || this.isPreparing || this.isBlocking)
            return;
        
        this.isDodging = true;
        this.dodgeDirection = direction;
        this.cooldownService.TriggerCooldown(cooldownId);
        this.view.PlayDodgeAnimation(direction);
    }
    
    #endregion
    
    #region Combat Actions
    
    public void StartPrepareAttack(bool isRightHand)
    {
        if (!this.CanAttack) return;
        
        this.isPreparing = true;
        this.prepareProgress = 0f;
        this.view.StartPrepareAnimation(isRightHand);
    }
    
    public void ExecuteAttack(bool isRightHand)
    {
        if (!this.isPreparing) return;
        
        this.isPreparing = false;
        string cooldownId = isRightHand ? "punch_right" : "punch_left";
        this.cooldownService.TriggerCooldown(cooldownId);
        
        this.view.PlayAttackAnimation(isRightHand, this.prepareProgress);
        
        // Notify attack executed - actual damage resolved by BattleService
        this.signalBus.Fire(new PlayerAttackSignal { IsRightHand = isRightHand, PrepareProgress = this.prepareProgress });
    }
    
    public void CancelPrepare()
    {
        this.isPreparing = false;
        this.prepareProgress = 0f;
        this.view.CancelPrepareAnimation();
    }
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Start()
    {
        this.RegisterCooldowns();
    }
    
    private void Update()
    {
        if (this.isPreparing)
        {
            this.prepareProgress = Mathf.Min(1f, this.prepareProgress + Time.deltaTime / CombatConstants.PunchPrepareTime);
        }
        
        if (this.isBlocking && !this.cooldownService.IsReady("block"))
        {
            // Still in block animation
        }
        else if (this.isBlocking)
        {
            this.isBlocking = false;
            this.view.EndBlockAnimation();
        }
        
        if (this.isDodging)
        {
            // Dodge animation handles timing
        }
    }
    
    #endregion
    
    #region Private Methods
    
    private void RegisterCooldowns()
    {
        this.cooldownService.RegisterCooldown("punch_right", CombatConstants.PunchReloadTime);
        this.cooldownService.RegisterCooldown("punch_left", CombatConstants.PunchReloadTime);
        this.cooldownService.RegisterCooldown("block", CombatConstants.BlockReloadTime);
        this.cooldownService.RegisterCooldown("dodge_left", CombatConstants.DodgeReloadTime);
        this.cooldownService.RegisterCooldown("dodge_right", CombatConstants.DodgeReloadTime);
    }
    
    private IWeapon GetEquippedWeapon()
    {
        var data = this.equipmentService.GetEquipped(EquipmentSlot.MeleeRight);
        return data != null ? WeaponFactory.Create(data) : null;
    }
    
    private IArmor GetEquippedArmor()
    {
        var data = this.equipmentService.GetEquipped(EquipmentSlot.ArmorTop);
        // Convert EquipmentData to IArmor
        return null; // Implement ArmorAdapter
    }
    
    #endregion
}

// PlayerAttackSignal - add to CombatSignals.cs
public class PlayerAttackSignal
{
    public bool IsRightHand { get; set; }
    public float PrepareProgress { get; set; }
}
```

### 2. Player Combat View

```csharp
// PlayerCombatView.cs
public class PlayerCombatView : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Transform modelRoot;
    
    // Animation parameter hashes
    private static readonly int PrepareHash = Animator.StringToHash("Prepare");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int BlockHash = Animator.StringToHash("Block");
    private static readonly int DodgeLeftHash = Animator.StringToHash("DodgeLeft");
    private static readonly int DodgeRightHash = Animator.StringToHash("DodgeRight");
    private static readonly int HitHash = Animator.StringToHash("Hit");
    private static readonly int DeathHash = Animator.StringToHash("Death");
    
    public void StartPrepareAnimation(bool isRightHand)
    {
        this.animator.SetBool(PrepareHash, true);
        // Could differentiate left/right hand
    }
    
    public void PlayAttackAnimation(bool isRightHand, float prepareProgress)
    {
        this.animator.SetBool(PrepareHash, false);
        this.animator.SetTrigger(AttackHash);
        // prepareProgress affects damage, animation can reflect this
    }
    
    public void CancelPrepareAnimation()
    {
        this.animator.SetBool(PrepareHash, false);
    }
    
    public void PlayBlockAnimation()
    {
        this.animator.SetTrigger(BlockHash);
    }
    
    public void EndBlockAnimation()
    {
        // Transition back to idle handled by Animator
    }
    
    public void PlayDodgeAnimation(DodgeDirection direction)
    {
        this.animator.SetTrigger(direction == DodgeDirection.Left ? DodgeLeftHash : DodgeRightHash);
    }
    
    public void PlayDamageAnimation(DamageResult damage)
    {
        this.animator.SetTrigger(HitHash);
        // Could spawn damage numbers VFX here
    }
    
    public void PlayDeathAnimation()
    {
        this.animator.SetTrigger(DeathHash);
    }
}
```

### 3. Weapons

```csharp
// WeaponBase.cs
public abstract class WeaponBase : IWeapon
{
    public string Id { get; protected set; }
    public WeaponType Type { get; protected set; }
    public float BaseDamage { get; protected set; }
    public float CriticalChance { get; protected set; }
    public float CriticalMultiplier { get; protected set; }
    public float ReloadTime { get; protected set; }
    public float PrepareTime { get; protected set; }
    
    protected WeaponBase(EquipmentData data)
    {
        Id = data.Id;
        BaseDamage = data.Damage;
        CriticalChance = data.CriticalChance;
        CriticalMultiplier = data.CriticalMultiplier;
        ReloadTime = data.ReloadTime;
        PrepareTime = CombatConstants.PunchPrepareTime;
    }
}

// MeleeWeapon.cs
public class MeleeWeapon : WeaponBase
{
    public MeleeWeaponType MeleeType { get; }
    
    public MeleeWeapon(EquipmentData data, MeleeWeaponType meleeType = MeleeWeaponType.DefaultFist) 
        : base(data)
    {
        Type = WeaponType.Melee;
        MeleeType = meleeType;
    }
}

public enum MeleeWeaponType
{
    DefaultFist,
    GripDefault,
    GripSword,
    GripSwordBig
}

// RangedWeapon.cs
public class RangedWeapon : WeaponBase
{
    public float ProjectileSpeed { get; }
    public float SplashRadius { get; }
    
    public RangedWeapon(EquipmentData data, float projectileSpeed = 20f, float splashRadius = 0f) 
        : base(data)
    {
        Type = data.Id.Contains("minigun") ? WeaponType.MachineGun : WeaponType.Gun;
        ProjectileSpeed = projectileSpeed;
        SplashRadius = splashRadius;
    }
}

// ShieldWeapon.cs
public class ShieldWeapon : WeaponBase, IArmor
{
    public float Defense { get; }
    public float CurrentDurability { get; private set; }
    public float MaxDurability { get; }
    
    public ShieldWeapon(EquipmentData data) : base(data)
    {
        Type = WeaponType.Melee; // Shield is defensive
        Defense = data.Defense;
        MaxDurability = 100f;
        CurrentDurability = MaxDurability;
    }
    
    public float GetDamageReduction()
    {
        float durabilityPercent = CurrentDurability / MaxDurability;
        return Defense * durabilityPercent;
    }
    
    public void TakeDamage(float amount)
    {
        CurrentDurability = Mathf.Max(0, CurrentDurability - amount);
    }
}

// WeaponFactory.cs
public static class WeaponFactory
{
    public static IWeapon Create(EquipmentData data)
    {
        if (data == null) return null;
        
        return data.Slot switch
        {
            EquipmentSlot.MeleeLeft or EquipmentSlot.MeleeRight => new MeleeWeapon(data),
            EquipmentSlot.Ranged => new RangedWeapon(data),
            EquipmentSlot.Shield => new ShieldWeapon(data),
            _ => null
        };
    }
}
```

---

## Verification Checklist

- [ ] PlayerCombatController correctly implements all interfaces
- [ ] Attack preparation tracks time correctly
- [ ] Block and dodge have correct cooldowns
- [ ] WeaponFactory creates correct weapon types
- [ ] View animations trigger correctly
- [ ] VContainer injection works (method injection via Construct)

---

## Notes

- Method injection via `[Inject] Construct()` for MonoBehaviour (VContainer)
- View is separate MonoBehaviour, referenced via SerializeField
- Actual damage resolution happens in BattleService (Phase 8), not here
- PlayerAttackSignal notifies system, doesn't directly damage enemy
