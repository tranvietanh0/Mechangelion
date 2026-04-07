# Codebase Summary

## Quick Start

1. Open the project with Unity `6000.3.10f1`.
2. Let Package Manager restore dependencies.
3. Open `Assets/Scenes/0.LoadingScene.unity` and press Play.
4. The loading flow initializes user data, then loads `1.MainScene` through `IGameAssets`.

## What This Repo Is

- `UnityMechangelion` is currently a thin local game layer on top of shared submodules.
- Most reusable framework code lives in `Assets/Submodules/GameFoundationCore/`, `Assets/Submodules/UITemplate/`, `Assets/Submodules/Extensions/`, and `Assets/Submodules/Logging/`.
- Most project-specific code lives in `Assets/Scripts/`.

## Repo Map

```text
UnityMechangelion/
|- Assets/
|  |- Scenes/                    # Entry and gameplay scenes
|  |- Scripts/                   # Project-specific code
|  |  |- Scenes/                 # Root scope + scene scopes + loading screen
|  |  |- Models/                 # Local data and other local models
|  |  `- StateMachines/          # Game state machine and game states
|  |- AddressableAssetsData/     # Addressables settings and groups
|  `- Submodules/                # Shared frameworks and utilities
|     |- GameFoundationCore/
|     |- UITemplate/
|     |- Extensions/
|     `- Logging/
|- Packages/
|- ProjectSettings/
|- docs/
`- plans/
```

## Current Runtime Flow

```text
0.LoadingScene
  -> LoadingSceneScope
  -> InitScreenManually<LoadingScreenPresenter>()
  -> LoadingScreenPresenter.BindData()
  -> UserDataManager.LoadUserData()
  -> IGameAssets.LoadSceneAsync("1.MainScene")
  -> MainSceneScope
  -> GameStateMachine.Initialize()
  -> TransitionTo<GameHomeState>()
```

## Key Entry Points

| File | Purpose |
|------|---------|
| `Assets/Scripts/Scenes/GameLifetimeScope.cs` | Root DI registration for GameFoundation + UITemplate |
| `Assets/Scripts/Scenes/Loading/LoadingSceneScope.cs` | Loading scene bootstrapping via manual screen initialization |
| `Assets/Scripts/Scenes/Screen/LoadingScreenView.cs` | Loading screen view + presenter; loads user data and main scene |
| `Assets/Scripts/Scenes/Main/MainSceneScope.cs` | Main scene registration and reflection-based state creation |
| `Assets/Scripts/StateMachines/Game/GameStateMachine.cs` | Local gameplay state machine bootstrap |
| `Assets/Submodules/GameFoundationCore/Scripts/UIModule/ScreenFlow/Manager/ScreenManager.cs` | Screen loading, presenter/view binding, manual screen support |

## Important Project Contracts

- `0.LoadingScene` is the only startup scene in build settings.
- `1.MainScene` is currently loaded through Addressables, not through build index flow.
- `RootUICanvas` must exist in scenes that use the screen system.
- `IGameState` implementations are discovered through reflection in `MainSceneScope`.
- Shared submodules are framework code; local feature work should default to `Assets/Scripts/` unless intentionally changing the framework.

## Common Tasks

### Add a New Screen

1. Create a `BaseView` subclass and a matching presenter.
2. Add `[ScreenInfo(nameof(MyScreenView))]` to the presenter.
3. By convention, keep the prefab/addressable path aligned with the view name for normal screens.
4. Open it with `await screenManager.OpenScreen<MyScreenPresenter>();`.

### Add a New Scene-Embedded Loading-Style Screen

1. Place the view under `RootUICanvas.RootUIShowTransform` in the scene hierarchy.
2. Create the presenter with `[ScreenInfo(nameof(MyScreenView))]`.
3. Register it with `builder.InitScreenManually<MyScreenPresenter>(autoBindData: true);`.

### Add a New Game State

1. Create a class implementing `IGameState`.
2. Inject dependencies through the constructor.
3. Prefer putting the class in the local game assembly for project ownership and predictability.
4. Transition through the owning state machine, for example `this.StateMachine.TransitionTo<MyState>()` when the state implements `IHaveStateMachine`.

### Add a New Signal

1. Define a signal class.
2. Register it with `builder.DeclareSignal<MySignal>();` in the relevant scope/registration extension.
3. Subscribe and unsubscribe through `SignalBus` in the owner lifecycle.

### Add Config Data

1. Create the CSV/config source used by BlueprintFlow.
2. Create the model.
3. Create a reader with `[BlueprintReader("ConfigName")]`.

## Current Tooling

- DI: `VContainer`
- Async: `UniTask`
- Messaging: `MessagePipe` through `SignalBus`
- Asset loading: `Addressables` through `IGameAssets`
- Config/data pipeline: BlueprintFlow
- Logging/utilities: `UniT.*`

## Current Gaps

- No checked-in unit test assembly is present yet.
- `docs/architecture-design.md` and `plans/20260407-mechangelion-rebuild/PLAN.md` describe target-state work, not current runtime truth.

## Related Docs

- [System Architecture](system-architecture.md) - Current runtime architecture and flow
- [Code Standards](code-standards.md) - Enforceable project coding rules
- [Architecture Design](architecture-design.md) - Target-state / rebuild design, not current runtime truth
- [Development Roadmap](development-roadmap.md) - Planned work
- [Project Structure - CrazyGame](project-structure-crazygame.md) - Legacy/source reference, not current repo structure
