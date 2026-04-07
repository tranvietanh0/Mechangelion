# Phase 06: Input Feature

**Effort:** S (1 day)
**Dependencies:** Phase 4 (Player combat controller)
**Blocked By:** Phase 4

---

## Objective

Implement input handling: service, combat input controller, button availability.

---

## File Ownership

```
Assets/Scripts/Features/Input/
├── Interfaces/
│   ├── IInputHandler.cs            [NEW]
│   └── ICombatInput.cs             [NEW]
│
├── Services/
│   └── InputService.cs             [NEW]
│
└── Controllers/
    ├── CombatInputController.cs    [NEW]
    └── ButtonAvailability.cs       [NEW]
```

**Total Files:** 5

---

## Implementation Details

### 1. Interfaces

```csharp
// IInputHandler.cs
public interface IInputHandler
{
    bool IsEnabled { get; set; }
    void ProcessInput();
}

// ICombatInput.cs
public interface ICombatInput
{
    // Attack
    event Action<bool> OnAttackPressed;     // bool = isRightHand
    event Action<bool> OnAttackReleased;
    
    // Defense
    event Action OnBlockPressed;
    event Action OnBlockReleased;
    
    // Movement
    event Action<DodgeDirection> OnDodgePressed;
    
    // State
    bool CanAttackRight { get; }
    bool CanAttackLeft { get; }
    bool CanBlock { get; }
    bool CanDodgeLeft { get; }
    bool CanDodgeRight { get; }
}
```

### 2. Input Service

```csharp
// InputService.cs
public class InputService : IInputHandler, ITickable
{
    private readonly List<IInputHandler> handlers = new();
    
    public bool IsEnabled { get; set; } = true;
    
    public void RegisterHandler(IInputHandler handler)
    {
        if (!this.handlers.Contains(handler))
            this.handlers.Add(handler);
    }
    
    public void UnregisterHandler(IInputHandler handler)
    {
        this.handlers.Remove(handler);
    }
    
    public void ProcessInput()
    {
        if (!IsEnabled) return;
        
        foreach (var handler in this.handlers)
        {
            if (handler.IsEnabled)
                handler.ProcessInput();
        }
    }
    
    // ITickable - called every frame
    public void Tick()
    {
        ProcessInput();
    }
    
    public void DisableAll()
    {
        foreach (var handler in this.handlers)
            handler.IsEnabled = false;
    }
    
    public void EnableAll()
    {
        foreach (var handler in this.handlers)
            handler.IsEnabled = true;
    }
}
```

### 3. Combat Input Controller

```csharp
// CombatInputController.cs
public class CombatInputController : IInputHandler, ICombatInput, IInitializable, IDisposable
{
    private readonly CooldownService cooldownService;
    private readonly SignalBus signalBus;
    
    private bool isEnabled = true;
    private bool commonAvailable = true;
    
    // Track button states
    private bool attackRightHeld;
    private bool attackLeftHeld;
    private bool blockHeld;
    
    #region Events
    
    public event Action<bool> OnAttackPressed;
    public event Action<bool> OnAttackReleased;
    public event Action OnBlockPressed;
    public event Action OnBlockReleased;
    public event Action<DodgeDirection> OnDodgePressed;
    
    #endregion
    
    #region Properties
    
    public bool IsEnabled
    {
        get => this.isEnabled;
        set => this.isEnabled = value;
    }
    
    public bool CanAttackRight => this.commonAvailable && this.cooldownService.IsReady("punch_right");
    public bool CanAttackLeft => this.commonAvailable && this.cooldownService.IsReady("punch_left");
    public bool CanBlock => this.commonAvailable && this.cooldownService.IsReady("block");
    public bool CanDodgeLeft => this.commonAvailable && this.cooldownService.IsReady("dodge_left");
    public bool CanDodgeRight => this.commonAvailable && this.cooldownService.IsReady("dodge_right");
    
    #endregion
    
    public CombatInputController(CooldownService cooldownService, SignalBus signalBus)
    {
        this.cooldownService = cooldownService;
        this.signalBus = signalBus;
    }
    
    public void Initialize()
    {
        // Subscribe to game state signals to enable/disable input
        this.signalBus.Subscribe<BattleStartedSignal>(OnBattleStarted);
        this.signalBus.Subscribe<BattleEndedSignal>(OnBattleEnded);
    }
    
    public void Dispose()
    {
        this.signalBus.Unsubscribe<BattleStartedSignal>(OnBattleStarted);
        this.signalBus.Unsubscribe<BattleEndedSignal>(OnBattleEnded);
    }
    
    public void ProcessInput()
    {
        if (!this.isEnabled) return;
        
        ProcessAttackInput();
        ProcessBlockInput();
        ProcessDodgeInput();
    }
    
    #region Input Processing
    
    private void ProcessAttackInput()
    {
        // Right attack (example: right mouse button or touch on right side)
        if (UnityEngine.Input.GetMouseButtonDown(0) && IsRightSideOfScreen())
        {
            if (CanAttackRight && !this.attackRightHeld)
            {
                this.attackRightHeld = true;
                OnAttackPressed?.Invoke(true);
            }
        }
        
        if (UnityEngine.Input.GetMouseButtonUp(0) && this.attackRightHeld)
        {
            this.attackRightHeld = false;
            OnAttackReleased?.Invoke(true);
        }
        
        // Left attack (left side of screen or left mouse)
        if (UnityEngine.Input.GetMouseButtonDown(1) || 
            (UnityEngine.Input.GetMouseButtonDown(0) && !IsRightSideOfScreen()))
        {
            if (CanAttackLeft && !this.attackLeftHeld)
            {
                this.attackLeftHeld = true;
                OnAttackPressed?.Invoke(false);
            }
        }
        
        if ((UnityEngine.Input.GetMouseButtonUp(1) || UnityEngine.Input.GetMouseButtonUp(0)) && this.attackLeftHeld)
        {
            this.attackLeftHeld = false;
            OnAttackReleased?.Invoke(false);
        }
    }
    
    private void ProcessBlockInput()
    {
        // Block (spacebar or swipe down)
        if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
        {
            if (CanBlock && !this.blockHeld)
            {
                this.blockHeld = true;
                OnBlockPressed?.Invoke();
            }
        }
        
        if (UnityEngine.Input.GetKeyUp(KeyCode.Space) && this.blockHeld)
        {
            this.blockHeld = false;
            OnBlockReleased?.Invoke();
        }
    }
    
    private void ProcessDodgeInput()
    {
        // Dodge left (A key or swipe left)
        if (UnityEngine.Input.GetKeyDown(KeyCode.A))
        {
            if (CanDodgeLeft)
            {
                OnDodgePressed?.Invoke(DodgeDirection.Left);
            }
        }
        
        // Dodge right (D key or swipe right)
        if (UnityEngine.Input.GetKeyDown(KeyCode.D))
        {
            if (CanDodgeRight)
            {
                OnDodgePressed?.Invoke(DodgeDirection.Right);
            }
        }
    }
    
    #endregion
    
    #region Helpers
    
    private bool IsRightSideOfScreen()
    {
        return UnityEngine.Input.mousePosition.x > Screen.width / 2f;
    }
    
    private void OnBattleStarted(BattleStartedSignal signal)
    {
        this.isEnabled = true;
        this.commonAvailable = true;
    }
    
    private void OnBattleEnded(BattleEndedSignal signal)
    {
        this.isEnabled = false;
        this.commonAvailable = false;
    }
    
    #endregion
    
    #region Public Control
    
    public void SetCommonAvailable(bool available)
    {
        this.commonAvailable = available;
    }
    
    #endregion
}
```

### 4. Button Availability

```csharp
// ButtonAvailability.cs
public class ButtonAvailability
{
    private readonly CooldownService cooldownService;
    private bool globalEnabled = true;
    
    public ButtonAvailability(CooldownService cooldownService)
    {
        this.cooldownService = cooldownService;
    }
    
    public bool IsAttackRightAvailable => this.globalEnabled && this.cooldownService.IsReady("punch_right");
    public bool IsAttackLeftAvailable => this.globalEnabled && this.cooldownService.IsReady("punch_left");
    public bool IsBlockAvailable => this.globalEnabled && this.cooldownService.IsReady("block");
    public bool IsDodgeLeftAvailable => this.globalEnabled && this.cooldownService.IsReady("dodge_left");
    public bool IsDodgeRightAvailable => this.globalEnabled && this.cooldownService.IsReady("dodge_right");
    
    public float GetAttackRightCooldownPercent()
    {
        float remaining = this.cooldownService.GetRemainingTime("punch_right");
        return remaining / CombatConstants.PunchReloadTime;
    }
    
    public float GetAttackLeftCooldownPercent()
    {
        float remaining = this.cooldownService.GetRemainingTime("punch_left");
        return remaining / CombatConstants.PunchReloadTime;
    }
    
    public float GetBlockCooldownPercent()
    {
        float remaining = this.cooldownService.GetRemainingTime("block");
        return remaining / CombatConstants.BlockReloadTime;
    }
    
    public float GetDodgeCooldownPercent(DodgeDirection direction)
    {
        string id = direction == DodgeDirection.Left ? "dodge_left" : "dodge_right";
        float remaining = this.cooldownService.GetRemainingTime(id);
        return remaining / CombatConstants.DodgeReloadTime;
    }
    
    public void SetGlobalEnabled(bool enabled)
    {
        this.globalEnabled = enabled;
    }
}
```

---

## DI Registration

```csharp
// MainSceneScope.Configure()
builder.Register<InputService>(Lifetime.Scoped).AsInterfacesAndSelf();
builder.Register<CombatInputController>(Lifetime.Scoped).AsInterfacesAndSelf();
builder.Register<ButtonAvailability>(Lifetime.Scoped);
```

---

## Wiring to Player

```csharp
// In PlayerCombatController or a mediator class
public class PlayerInputMediator : IInitializable, IDisposable
{
    private readonly ICombatInput combatInput;
    private readonly PlayerCombatController player;
    
    public PlayerInputMediator(ICombatInput combatInput, PlayerCombatController player)
    {
        this.combatInput = combatInput;
        this.player = player;
    }
    
    public void Initialize()
    {
        this.combatInput.OnAttackPressed += OnAttackPressed;
        this.combatInput.OnAttackReleased += OnAttackReleased;
        this.combatInput.OnBlockPressed += OnBlockPressed;
        this.combatInput.OnDodgePressed += OnDodgePressed;
    }
    
    public void Dispose()
    {
        this.combatInput.OnAttackPressed -= OnAttackPressed;
        this.combatInput.OnAttackReleased -= OnAttackReleased;
        this.combatInput.OnBlockPressed -= OnBlockPressed;
        this.combatInput.OnDodgePressed -= OnDodgePressed;
    }
    
    private void OnAttackPressed(bool isRightHand) => this.player.StartPrepareAttack(isRightHand);
    private void OnAttackReleased(bool isRightHand) => this.player.ExecuteAttack(isRightHand);
    private void OnBlockPressed() => this.player.TryBlock();
    private void OnDodgePressed(DodgeDirection dir) => this.player.Dodge(dir);
}
```

---

## Verification Checklist

- [ ] InputService processes all registered handlers
- [ ] CombatInputController fires correct events
- [ ] ButtonAvailability returns correct cooldown states
- [ ] Input disabled during battle end
- [ ] Screen-side detection works for mobile

---

## Notes

- Current implementation uses Unity's old Input system
- Can be upgraded to New Input System later
- Touch input can be added in ProcessInput methods
- Mediator pattern decouples input from player
