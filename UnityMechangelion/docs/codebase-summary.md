# Codebase Summary

## Quick Start

1. Open project in Unity Hub with Unity 6000.3.10f1
2. Let packages restore (VContainer, UniTask, MessagePipe via OpenUPM)
3. Open `0.LoadingScene` and press Play

## Project Structure

```
UnityMechangelion/
├── Assets/
│   ├── Scripts/           # Game-specific code
│   ├── Submodules/        # Shared libraries (git submodules)
│   │   ├── GameFoundationCore/  # Core framework
│   │   ├── UITemplate/          # UI system
│   │   ├── Extensions/          # Utility extensions
│   │   └── Logging/             # Logging utilities
│   └── Addressables/      # Addressable assets config
├── Packages/              # Unity packages
└── ProjectSettings/       # Unity project settings
```

## Key Entry Points

| File | Purpose |
|------|---------|
| `GameLifetimeScope` | Root DI container setup |
| `LoadingSceneScope` | Loading scene initialization |
| `MainSceneScope` | Main game scene setup |
| `GameStateMachine` | Main game flow controller |

## Common Tasks

### Add a New Screen

1. Create View: `public class MyView : BaseView { }`
2. Create Presenter: `[ScreenInfo(nameof(MyView))] public class MyPresenter : BaseScreenPresenter<MyView> { }`
3. Open screen: `await screenManager.OpenScreen<MyPresenter>();`

### Add a New Game State

1. Create state: `public class MyState : IGameState { }`
2. Implement `Enter()` and `Exit()`
3. States auto-register via reflection

### Add a New Signal

1. Define: `public struct MySignal { }`
2. Register in LifetimeScope: `builder.DeclareSignal<MySignal>();`
3. Subscribe/Fire via `SignalBus`

### Add Config Data

1. Create CSV in `Blueprints/` folder
2. Create model class
3. Create reader: `[BlueprintReader("ConfigName")] class MyReader : GenericBlueprintReaderByRow<MyModel> { }`

## Dependencies

- **VContainer** — Dependency injection
- **UniTask** — Async/await for Unity
- **MessagePipe** — Pub/sub messaging (SignalBus wrapper)
- **Addressables** — Asset loading
- **Newtonsoft.Json** — JSON serialization

## Related Docs

- [System Architecture](system-architecture.md) — Technical deep-dive
- [Code Standards](code-standards.md) — Coding conventions
- [Development Roadmap](development-roadmap.md) — Planned features
