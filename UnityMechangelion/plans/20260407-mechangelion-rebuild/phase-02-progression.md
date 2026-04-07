# Phase 02: Progression Feature

**Effort:** M (2 days)
**Dependencies:** Phase 1 (Core interfaces, enums, signals)
**Blocked By:** Phase 1

---

## Objective

Implement progression system: level, currency, equipment inventory, upgrades.

---

## File Ownership

```
Assets/Scripts/Features/Progression/
├── Interfaces/
│   ├── ILevelable.cs               [NEW]
│   ├── ICurrency.cs                [NEW]
│   └── IEquipment.cs               [NEW]
│
├── Models/
│   ├── PlayerProgress.cs           [NEW]
│   ├── CurrencyData.cs             [NEW]
│   ├── EquipmentData.cs            [NEW]
│   └── UpgradeCost.cs              [NEW]
│
├── Services/
│   ├── ProgressionService.cs       [NEW]
│   ├── CurrencyService.cs          [NEW]
│   ├── EquipmentService.cs         [NEW]
│   └── UpgradeService.cs           [NEW]
```

**Total Files:** 12

---

## Implementation Details

### 1. Interfaces

```csharp
// ILevelable.cs
public interface ILevelable
{
    int CurrentLevel { get; }
    int CurrentXP { get; }
    int XPToNextLevel { get; }
    void AddXP(int amount);
    event Action<int> OnLevelUp;
}

// ICurrency.cs
public interface ICurrency
{
    int GetAmount(CurrencyType type);
    bool TrySpend(CurrencyType type, int amount);
    void Add(CurrencyType type, int amount);
    event Action<CurrencyType, int, int> OnCurrencyChanged;
}

// IEquipment.cs
public interface IEquipment
{
    string Id { get; }
    EquipmentSlot Slot { get; }
    int Level { get; }
    int Rarity { get; }
    float GetStat(string statName);
}
```

### 2. Models

```csharp
// PlayerProgress.cs
[Serializable]
public class PlayerProgress
{
    public int Level { get; set; } = 1;
    public int XP { get; set; } = 0;
    public int TotalBattles { get; set; } = 0;
    public int Victories { get; set; } = 0;
    
    public int XPToNextLevel => CalculateXPRequired(Level);
    
    private int CalculateXPRequired(int level)
    {
        return 100 * level; // Simple formula, adjust as needed
    }
}

// CurrencyData.cs
[Serializable]
public class CurrencyData
{
    public int Coins { get; set; } = 0;
    public int Cores { get; set; } = 0;
    
    public int GetAmount(CurrencyType type) => type switch
    {
        CurrencyType.Coins => Coins,
        CurrencyType.Cores => Cores,
        _ => 0
    };
}

// EquipmentData.cs
[Serializable]
public class EquipmentData : IEquipment
{
    public string Id { get; set; }
    public EquipmentSlot Slot { get; set; }
    public int Level { get; set; } = 1;
    public int Rarity { get; set; } = 1;
    public int Quantity { get; set; } = 1;
    
    // Stats
    public float Damage { get; set; }
    public float Defense { get; set; }
    public float CriticalChance { get; set; }
    public float CriticalMultiplier { get; set; } = 1.5f;
    public float ReloadTime { get; set; }
    
    public float GetStat(string statName) => statName.ToLower() switch
    {
        "damage" => Damage * Level,
        "defense" => Defense * Level,
        "critchance" => CriticalChance,
        "critmult" => CriticalMultiplier,
        "reload" => ReloadTime,
        _ => 0f
    };
}

// UpgradeCost.cs
public static class UpgradeCost
{
    public static int Calculate(int currentLevel)
    {
        int tier = currentLevel / BalanceConstants.UpgradeIncreaseEvery;
        int cost = BalanceConstants.UpgradeBaseCost + (tier * 100);
        return Math.Min(cost, BalanceConstants.UpgradeMaxCost);
    }
}
```

### 3. Services

```csharp
// ProgressionService.cs
public class ProgressionService : ILevelable, IInitializable
{
    private readonly ISaveService saveService;
    private readonly SignalBus signalBus;
    private PlayerProgress progress;
    
    private const string SaveKey = "player_progress";
    
    public int CurrentLevel => this.progress.Level;
    public int CurrentXP => this.progress.XP;
    public int XPToNextLevel => this.progress.XPToNextLevel;
    
    public event Action<int> OnLevelUp;
    
    public ProgressionService(ISaveService saveService, SignalBus signalBus)
    {
        this.saveService = saveService;
        this.signalBus = signalBus;
    }
    
    public void Initialize()
    {
        this.progress = this.saveService.Load<PlayerProgress>(SaveKey) ?? new PlayerProgress();
    }
    
    public void AddXP(int amount)
    {
        this.progress.XP += amount;
        while (this.progress.XP >= this.progress.XPToNextLevel)
        {
            this.progress.XP -= this.progress.XPToNextLevel;
            this.progress.Level++;
            this.OnLevelUp?.Invoke(this.progress.Level);
            this.signalBus.Fire(new LevelUpSignal { NewLevel = this.progress.Level, OldLevel = this.progress.Level - 1 });
        }
        this.Save();
    }
    
    private void Save() => this.saveService.Save(SaveKey, this.progress);
}

// CurrencyService.cs
public class CurrencyService : ICurrency, IInitializable
{
    private readonly ISaveService saveService;
    private readonly SignalBus signalBus;
    private CurrencyData data;
    
    private const string SaveKey = "currency_data";
    
    public event Action<CurrencyType, int, int> OnCurrencyChanged;
    
    public CurrencyService(ISaveService saveService, SignalBus signalBus)
    {
        this.saveService = saveService;
        this.signalBus = signalBus;
    }
    
    public void Initialize()
    {
        this.data = this.saveService.Load<CurrencyData>(SaveKey) ?? new CurrencyData();
    }
    
    public int GetAmount(CurrencyType type) => this.data.GetAmount(type);
    
    public bool TrySpend(CurrencyType type, int amount)
    {
        int current = this.GetAmount(type);
        if (current < amount) return false;
        
        this.SetAmount(type, current - amount);
        return true;
    }
    
    public void Add(CurrencyType type, int amount)
    {
        int current = this.GetAmount(type);
        this.SetAmount(type, current + amount);
    }
    
    private void SetAmount(CurrencyType type, int newAmount)
    {
        int oldAmount = this.GetAmount(type);
        switch (type)
        {
            case CurrencyType.Coins: this.data.Coins = newAmount; break;
            case CurrencyType.Cores: this.data.Cores = newAmount; break;
        }
        this.OnCurrencyChanged?.Invoke(type, oldAmount, newAmount);
        this.signalBus.Fire(new CurrencyChangedSignal { Type = type, OldAmount = oldAmount, NewAmount = newAmount });
        this.Save();
    }
    
    private void Save() => this.saveService.Save(SaveKey, this.data);
}

// EquipmentService.cs
public class EquipmentService : IInitializable
{
    private readonly ISaveService saveService;
    private Dictionary<string, EquipmentData> inventory = new();
    private Dictionary<EquipmentSlot, string> equipped = new();
    
    private const string InventoryKey = "equipment_inventory";
    private const string EquippedKey = "equipment_equipped";
    
    public IReadOnlyDictionary<string, EquipmentData> Inventory => this.inventory;
    
    public EquipmentService(ISaveService saveService)
    {
        this.saveService = saveService;
    }
    
    public void Initialize()
    {
        this.inventory = this.saveService.Load<Dictionary<string, EquipmentData>>(InventoryKey) 
            ?? new Dictionary<string, EquipmentData>();
        this.equipped = this.saveService.Load<Dictionary<EquipmentSlot, string>>(EquippedKey)
            ?? new Dictionary<EquipmentSlot, string>();
    }
    
    public void AddEquipment(EquipmentData equipment)
    {
        if (this.inventory.ContainsKey(equipment.Id))
            this.inventory[equipment.Id].Quantity++;
        else
            this.inventory[equipment.Id] = equipment;
        this.Save();
    }
    
    public EquipmentData GetEquipped(EquipmentSlot slot)
    {
        if (this.equipped.TryGetValue(slot, out var id) && this.inventory.TryGetValue(id, out var eq))
            return eq;
        return null;
    }
    
    public void Equip(string equipmentId, EquipmentSlot slot)
    {
        if (!this.inventory.ContainsKey(equipmentId)) return;
        this.equipped[slot] = equipmentId;
        this.Save();
    }
    
    private void Save()
    {
        this.saveService.Save(InventoryKey, this.inventory);
        this.saveService.Save(EquippedKey, this.equipped);
    }
}

// UpgradeService.cs
public class UpgradeService
{
    private readonly EquipmentService equipmentService;
    private readonly CurrencyService currencyService;
    private readonly SignalBus signalBus;
    
    public UpgradeService(
        EquipmentService equipmentService,
        CurrencyService currencyService,
        SignalBus signalBus)
    {
        this.equipmentService = equipmentService;
        this.currencyService = currencyService;
        this.signalBus = signalBus;
    }
    
    public int GetUpgradeCost(string equipmentId)
    {
        var eq = this.equipmentService.Inventory.GetValueOrDefault(equipmentId);
        return eq != null ? UpgradeCost.Calculate(eq.Level) : 0;
    }
    
    public bool CanUpgrade(string equipmentId)
    {
        int cost = this.GetUpgradeCost(equipmentId);
        return cost > 0 && this.currencyService.GetAmount(CurrencyType.Coins) >= cost;
    }
    
    public bool TryUpgrade(string equipmentId)
    {
        if (!this.CanUpgrade(equipmentId)) return false;
        
        int cost = this.GetUpgradeCost(equipmentId);
        if (!this.currencyService.TrySpend(CurrencyType.Coins, cost)) return false;
        
        var eq = this.equipmentService.Inventory[equipmentId];
        eq.Level++;
        
        this.signalBus.Fire(new EquipmentUpgradedSignal { EquipmentId = equipmentId, NewLevel = eq.Level });
        return true;
    }
}
```

---

## DI Registration (GameLifetimeScope)

```csharp
// Add to GameLifetimeScope.Configure()
builder.Register<ProgressionService>(Lifetime.Singleton).AsInterfacesAndSelf();
builder.Register<CurrencyService>(Lifetime.Singleton).AsInterfacesAndSelf();
builder.Register<EquipmentService>(Lifetime.Singleton).AsInterfacesAndSelf();
builder.Register<UpgradeService>(Lifetime.Singleton);

// Declare signals
builder.DeclareSignal<LevelUpSignal>();
builder.DeclareSignal<CurrencyChangedSignal>();
builder.DeclareSignal<EquipmentUpgradedSignal>();
```

---

## Verification Checklist

- [ ] ProgressionService loads/saves correctly
- [ ] CurrencyService fires signals on change
- [ ] EquipmentService manages inventory correctly
- [ ] UpgradeService calculates costs per BalanceConstants
- [ ] All services implement IInitializable
- [ ] No direct PlayerPrefs usage (only via ISaveService)

---

## Notes

- Services registered as Singleton vì progression data persistent across scenes
- IInitializable.Initialize() called by VContainer sau khi inject xong
- Data classes marked [Serializable] cho JSON persistence
