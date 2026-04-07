# Phase 09: UI

**Effort:** M (3 days)
**Dependencies:** Phase 8 (Battle feature)
**Blocked By:** Phase 8

---

## Objective

Implement UI screens: battle HUD, controls, health bars, menus.

---

## File Ownership

```
Assets/Scripts/UI/
├── Battle/
│   ├── BattleHUDView.cs            [NEW]
│   ├── BattleHUDPresenter.cs       [NEW]
│   ├── ControlButtonView.cs        [NEW]
│   └── BattleResultView.cs         [NEW]
│   └── BattleResultPresenter.cs    [NEW]
│
├── Menu/
│   ├── HomeScreenView.cs           [NEW]
│   └── HomeScreenPresenter.cs      [NEW]
│
└── Shared/
    ├── HealthBarView.cs            [NEW]
    └── CurrencyDisplayView.cs      [NEW]
```

**Total Files:** 10

---

## Implementation Details

### 1. Battle HUD

```csharp
// BattleHUDView.cs
public class BattleHUDView : BaseView
{
    [Header("Health Bars")]
    [SerializeField] private HealthBarView playerHealthBar;
    [SerializeField] private HealthBarView enemyHealthBar;
    
    [Header("Control Buttons")]
    [SerializeField] private ControlButtonView attackRightButton;
    [SerializeField] private ControlButtonView attackLeftButton;
    [SerializeField] private ControlButtonView blockButton;
    [SerializeField] private ControlButtonView dodgeLeftButton;
    [SerializeField] private ControlButtonView dodgeRightButton;
    
    [Header("Status")]
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private Image prepareIndicator;
    
    public HealthBarView PlayerHealthBar => this.playerHealthBar;
    public HealthBarView EnemyHealthBar => this.enemyHealthBar;
    
    public ControlButtonView AttackRightButton => this.attackRightButton;
    public ControlButtonView AttackLeftButton => this.attackLeftButton;
    public ControlButtonView BlockButton => this.blockButton;
    public ControlButtonView DodgeLeftButton => this.dodgeLeftButton;
    public ControlButtonView DodgeRightButton => this.dodgeRightButton;
    
    public void ShowCombo(int count)
    {
        this.comboText.text = $"x{count}";
        this.comboText.gameObject.SetActive(true);
        // Animate combo text
    }
    
    public void HideCombo()
    {
        this.comboText.gameObject.SetActive(false);
    }
    
    public void UpdatePrepareIndicator(float progress)
    {
        this.prepareIndicator.fillAmount = progress;
        this.prepareIndicator.gameObject.SetActive(progress > 0);
    }
}

// BattleHUDPresenter.cs
[ScreenInfo(nameof(BattleHUDView))]
public class BattleHUDPresenter : BaseScreenPresenter<BattleHUDView>, IDisposable
{
    private readonly BattleService battleService;
    private readonly ButtonAvailability buttonAvailability;
    private readonly ICombatInput combatInput;
    
    private PlayerCombatController player;
    private EnemyBase enemy;
    
    public BattleHUDPresenter(
        SignalBus signalBus,
        ILoggerManager loggerManager,
        BattleService battleService,
        ButtonAvailability buttonAvailability,
        ICombatInput combatInput)
        : base(signalBus, loggerManager)
    {
        this.battleService = battleService;
        this.buttonAvailability = buttonAvailability;
        this.combatInput = combatInput;
    }
    
    public override async UniTask BindData()
    {
        this.enemy = this.battleService.CurrentEnemy;
        // Get player reference from scene
        this.player = UnityEngine.Object.FindObjectOfType<PlayerCombatController>();
        
        // Setup health bars
        View.PlayerHealthBar.Initialize(this.player.MaxHealth);
        View.EnemyHealthBar.Initialize(this.enemy.MaxHealth);
        
        // Setup control buttons
        SetupControlButtons();
        
        // Subscribe to signals
        SignalBus.Subscribe<DamageDealtSignal>(OnDamageDealt);
        SignalBus.Subscribe<BattleEndedSignal>(OnBattleEnded);
    }
    
    public void Dispose()
    {
        SignalBus.Unsubscribe<DamageDealtSignal>(OnDamageDealt);
        SignalBus.Unsubscribe<BattleEndedSignal>(OnBattleEnded);
    }
    
    private void SetupControlButtons()
    {
        // Attack buttons
        View.AttackRightButton.OnPointerDown += () => this.combatInput.OnAttackPressed?.Invoke(true);
        View.AttackRightButton.OnPointerUp += () => this.combatInput.OnAttackReleased?.Invoke(true);
        
        View.AttackLeftButton.OnPointerDown += () => this.combatInput.OnAttackPressed?.Invoke(false);
        View.AttackLeftButton.OnPointerUp += () => this.combatInput.OnAttackReleased?.Invoke(false);
        
        // Block button
        View.BlockButton.OnPointerDown += () => this.combatInput.OnBlockPressed?.Invoke();
        View.BlockButton.OnPointerUp += () => this.combatInput.OnBlockReleased?.Invoke();
        
        // Dodge buttons
        View.DodgeLeftButton.OnClick += () => this.combatInput.OnDodgePressed?.Invoke(DodgeDirection.Left);
        View.DodgeRightButton.OnClick += () => this.combatInput.OnDodgePressed?.Invoke(DodgeDirection.Right);
    }
    
    public void Update()
    {
        // Update button states
        View.AttackRightButton.SetInteractable(this.buttonAvailability.IsAttackRightAvailable);
        View.AttackLeftButton.SetInteractable(this.buttonAvailability.IsAttackLeftAvailable);
        View.BlockButton.SetInteractable(this.buttonAvailability.IsBlockAvailable);
        View.DodgeLeftButton.SetInteractable(this.buttonAvailability.IsDodgeLeftAvailable);
        View.DodgeRightButton.SetInteractable(this.buttonAvailability.IsDodgeRightAvailable);
        
        // Update cooldown visuals
        View.AttackRightButton.SetCooldownProgress(this.buttonAvailability.GetAttackRightCooldownPercent());
        View.BlockButton.SetCooldownProgress(this.buttonAvailability.GetBlockCooldownPercent());
    }
    
    private void OnDamageDealt(DamageDealtSignal signal)
    {
        // Update health bars
        if (signal.Target == this.player as IDamageable)
        {
            View.PlayerHealthBar.SetValue(this.player.CurrentHealth);
        }
        else if (signal.Target == this.enemy as IDamageable)
        {
            View.EnemyHealthBar.SetValue(this.enemy.CurrentHealth);
        }
    }
    
    private void OnBattleEnded(BattleEndedSignal signal)
    {
        // Disable all buttons
        this.buttonAvailability.SetGlobalEnabled(false);
    }
}
```

### 2. Control Button View

```csharp
// ControlButtonView.cs
public class ControlButtonView : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Button button;
    [SerializeField] private Image icon;
    [SerializeField] private Image cooldownOverlay;
    [SerializeField] private CanvasGroup canvasGroup;
    
    public event Action OnPointerDown;
    public event Action OnPointerUp;
    public event Action OnClick;
    
    private bool isPressed;
    private bool interactable = true;
    
    public void SetInteractable(bool interactable)
    {
        this.interactable = interactable;
        this.canvasGroup.alpha = interactable ? 1f : 0.5f;
        this.button.interactable = interactable;
    }
    
    public void SetCooldownProgress(float progress)
    {
        this.cooldownOverlay.fillAmount = progress;
        this.cooldownOverlay.gameObject.SetActive(progress > 0);
    }
    
    public void SetIcon(Sprite sprite)
    {
        this.icon.sprite = sprite;
    }
    
    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        if (!this.interactable) return;
        this.isPressed = true;
        OnPointerDown?.Invoke();
    }
    
    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        if (!this.isPressed) return;
        this.isPressed = false;
        OnPointerUp?.Invoke();
        OnClick?.Invoke();
    }
}
```

### 3. Health Bar View

```csharp
// HealthBarView.cs
public class HealthBarView : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private Image delayedFillImage;
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private Gradient healthGradient;
    
    private float maxValue;
    private float currentValue;
    private float displayValue;
    
    private const float FillSpeed = 5f;
    
    public void Initialize(float maxValue)
    {
        this.maxValue = maxValue;
        this.currentValue = maxValue;
        this.displayValue = maxValue;
        UpdateVisual();
    }
    
    public void SetValue(float value)
    {
        this.currentValue = Mathf.Clamp(value, 0, this.maxValue);
        UpdateVisual();
    }
    
    public void SetValueInstant(float value)
    {
        this.currentValue = value;
        this.displayValue = value;
        UpdateVisual();
    }
    
    private void Update()
    {
        // Smooth delayed fill
        if (Mathf.Abs(this.displayValue - this.currentValue) > 0.01f)
        {
            this.displayValue = Mathf.MoveTowards(this.displayValue, this.currentValue, FillSpeed * Time.deltaTime * this.maxValue);
            this.delayedFillImage.fillAmount = this.displayValue / this.maxValue;
        }
    }
    
    private void UpdateVisual()
    {
        float percent = this.currentValue / this.maxValue;
        this.fillImage.fillAmount = percent;
        this.fillImage.color = this.healthGradient.Evaluate(percent);
        
        if (this.valueText != null)
        {
            this.valueText.text = $"{(int)this.currentValue}/{(int)this.maxValue}";
        }
    }
}
```

### 4. Battle Result Screen

```csharp
// BattleResultView.cs
public class BattleResultView : BaseView
{
    [Header("Result")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject defeatPanel;
    
    [Header("Rewards")]
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private TextMeshProUGUI xpText;
    [SerializeField] private Transform rewardsContainer;
    
    [Header("Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button reviveButton;
    
    public Button ContinueButton => this.continueButton;
    public Button RetryButton => this.retryButton;
    public Button ReviveButton => this.reviveButton;
    
    public void ShowVictory(RewardData rewards)
    {
        this.victoryPanel.SetActive(true);
        this.defeatPanel.SetActive(false);
        
        this.coinsText.text = $"+{rewards.Coins}";
        this.xpText.text = $"+{rewards.XP} XP";
        
        this.continueButton.gameObject.SetActive(true);
        this.retryButton.gameObject.SetActive(false);
        this.reviveButton.gameObject.SetActive(false);
    }
    
    public void ShowDefeat()
    {
        this.victoryPanel.SetActive(false);
        this.defeatPanel.SetActive(true);
        
        this.continueButton.gameObject.SetActive(true);
        this.retryButton.gameObject.SetActive(true);
        this.reviveButton.gameObject.SetActive(true); // Show if ad available
    }
}

// BattleResultPresenter.cs
[ScreenInfo(nameof(BattleResultView))]
public class BattleResultPresenter : BaseScreenPresenter<BattleResultView>
{
    private readonly BattleService battleService;
    private readonly RewardService rewardService;
    private readonly GameStateMachine stateMachine;
    
    private BattleResult result;
    
    public BattleResultPresenter(
        SignalBus signalBus,
        ILoggerManager loggerManager,
        BattleService battleService,
        RewardService rewardService,
        GameStateMachine stateMachine)
        : base(signalBus, loggerManager)
    {
        this.battleService = battleService;
        this.rewardService = rewardService;
        this.stateMachine = stateMachine;
    }
    
    public void SetResult(BattleResult result)
    {
        this.result = result;
    }
    
    public override UniTask BindData()
    {
        if (this.result.Victory)
        {
            View.ShowVictory(this.result.Rewards);
            this.rewardService.ApplyRewards(this.result.Rewards);
        }
        else
        {
            View.ShowDefeat();
        }
        
        View.ContinueButton.onClick.AddListener(OnContinue);
        View.RetryButton.onClick.AddListener(OnRetry);
        View.ReviveButton.onClick.AddListener(OnRevive);
        
        return UniTask.CompletedTask;
    }
    
    private void OnContinue()
    {
        this.stateMachine.TransitionTo<GameHomeState>();
    }
    
    private void OnRetry()
    {
        // Restart battle with same config
    }
    
    private void OnRevive()
    {
        // Show ad, then revive player
    }
}
```

### 5. Currency Display

```csharp
// CurrencyDisplayView.cs
public class CurrencyDisplayView : MonoBehaviour, IInitializable, IDisposable
{
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private TextMeshProUGUI coresText;
    
    private CurrencyService currencyService;
    private SignalBus signalBus;
    
    [Inject]
    public void Construct(CurrencyService currencyService, SignalBus signalBus)
    {
        this.currencyService = currencyService;
        this.signalBus = signalBus;
    }
    
    public void Initialize()
    {
        this.UpdateDisplay();
        this.signalBus.Subscribe<CurrencyChangedSignal>(this.OnCurrencyChanged);
    }
    
    public void Dispose()
    {
        this.signalBus.Unsubscribe<CurrencyChangedSignal>(OnCurrencyChanged);
    }
    
    private void OnCurrencyChanged(CurrencyChangedSignal signal)
    {
        UpdateDisplay();
        // Animate change
    }
    
    private void UpdateDisplay()
    {
        this.coinsText.text = FormatNumber(this.currencyService.GetAmount(CurrencyType.Coins));
        this.coresText.text = FormatNumber(this.currencyService.GetAmount(CurrencyType.Cores));
    }
    
    private string FormatNumber(int value)
    {
        if (value >= 1000000) return $"{value / 1000000f:F1}M";
        if (value >= 1000) return $"{value / 1000f:F1}K";
        return value.ToString();
    }
}
```

---

## Prefab Structure

```
UI/
├── Prefabs/
│   ├── BattleHUD.prefab
│   ├── BattleResult.prefab
│   ├── HomeScreen.prefab
│   └── Shared/
│       ├── HealthBar.prefab
│       ├── ControlButton.prefab
│       └── CurrencyDisplay.prefab
```

---

## Verification Checklist

- [ ] BattleHUD shows player and enemy health
- [ ] Control buttons fire correct events
- [ ] Cooldown overlays display correctly
- [ ] BattleResult shows rewards on victory
- [ ] Health bars animate smoothly
- [ ] Currency display updates reactively
