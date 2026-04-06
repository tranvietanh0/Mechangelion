# System Architecture

## Overview

Unity 6 hyper-casual game using VContainer for dependency injection, UniTask for async operations, and MessagePipe for pub/sub messaging.

## Technology Stack

| Layer | Technology |
|-------|-----------|
| Engine | Unity 6 (6000.3.10f1) |
| DI Container | VContainer |
| Async | UniTask |
| Pub/Sub | MessagePipe (SignalBus) |
| Asset Loading | Addressables |
| Data/Config | Blueprint System (CSV) |

## Assembly Structure

```
HyperCasualGame.Scripts          # Main game code
├── GameFoundationCore.Scripts   # Core framework
│   ├── .AssetLibrary            # Addressables wrapper
│   ├── .BlueprintFlow           # CSV config/data parsing
│   ├── .DI                      # DI interfaces
│   ├── .Models                  # Base data models
│   ├── .Signals                 # SignalBus implementation
│   ├── .UIModule                # Screen management, MVP
│   └── .Utilities               # ObjectPool, SoundManager, UserData
│
└── UITemplate.Scripts           # UI template system
    └── StateMachine             # State machine base
```

## Dependency Injection

### Scope Hierarchy

```
GameLifetimeScope (root)
└── SceneScope (per-scene)
    └── Feature-specific scopes
```

### Auto-registered Services

Via `RegisterGameFoundation()`:
- `SignalBus` — pub/sub messaging
- `IGameAssets` — Addressables wrapper
- `IScreenManager` — UI screen management
- `ObjectPoolManager` — object pooling
- `AudioService` — sound management
- `IHandleUserDataServices` — user data persistence

## Key Patterns

### State Machine

- States implement `IGameState`
- Auto-discovered via reflection
- Transition: `stateMachine.TransitionTo<TargetState>()`

### MVP (Model-View-Presenter)

- **View**: MonoBehaviour inheriting `BaseView`
- **Presenter**: Logic handler inheriting `BaseScreenPresenter<TView>`
- Open via `IScreenManager.OpenScreen<TPresenter>()`

### Signals

```csharp
// Declare
builder.DeclareSignal<MySignal>();

// Subscribe/Fire
signalBus.Subscribe<MySignal>(handler);
signalBus.Fire(new MySignal());
```

## Scene Flow

```
0.LoadingScene → Load user data → 1.MainScene
                                      ↓
                              GameStateMachine
                              manages game states
```

## Data Flow

```
CSV Config → BlueprintReader → Model → Game Logic
                                  ↓
User Actions → Signals → State Changes → UI Updates
```
