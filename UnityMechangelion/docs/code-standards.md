# Code Standards

## General Principles

- Follow SOLID principles
- Prefer composition over inheritance
- Use dependency injection for all services
- Keep classes focused (single responsibility)

## Naming Conventions

| Type | Convention | Example |
|------|-----------|---------|
| Class | PascalCase | `GameStateMachine` |
| Interface | IPascalCase | `IGameState` |
| Method | PascalCase | `TransitionTo()` |
| Property | PascalCase | `CurrentState` |
| Field (private) | _camelCase | `_screenManager` |
| Field (serialized) | camelCase | `startDelay` |
| Constant | UPPER_SNAKE | `MAX_HEALTH` |
| Enum | PascalCase | `GamePhase.Playing` |

## Project-Specific Patterns

### Dependency Injection

```csharp
// Constructor injection (preferred)
public class MyService
{
    private readonly IScreenManager _screenManager;
    
    public MyService(IScreenManager screenManager)
    {
        _screenManager = screenManager;
    }
}
```

### Async Operations

```csharp
// Use UniTask, not Task
public async UniTask LoadDataAsync()
{
    await UniTask.Delay(100);
}
```

### Screen Presenters

```csharp
[ScreenInfo(nameof(MyScreenView))]
public class MyScreenPresenter : BaseScreenPresenter<MyScreenView>
{
    public override async UniTask BindData()
    {
        // Initialize view
    }
}
```

### Game States

```csharp
public class PlayingState : IGameState, IHaveStateMachine
{
    public IStateMachine StateMachine { get; set; }
    
    public void Enter() { }
    public void Exit() { }
}
```

### Signals

```csharp
// Define signal
public struct GameStartedSignal { }

// Register in LifetimeScope
builder.DeclareSignal<GameStartedSignal>();

// Use
signalBus.Fire(new GameStartedSignal());
```

## Assembly References

- Main game code references `GameFoundationCore.*`
- Never create circular dependencies between assemblies
- UI code stays in `UITemplate` assembly

## File Organization

```
Assets/
├── Scripts/
│   ├── States/          # Game states
│   ├── Screens/         # UI presenters and views
│   ├── Services/        # Game services
│   └── Models/          # Data models
├── Submodules/          # Git submodules (read-only)
└── Addressables/        # Addressable assets
```

## Prohibited Practices

- Direct `new` for DI-managed services
- `Task` instead of `UniTask`
- Static singletons (use DI instead)
- Hard-coded magic numbers
- Empty catch blocks
