# System Architecture

## Overview

This project is a Unity 6 game shell built on shared internal frameworks. The current local game layer is small, but the runtime architecture is already defined by `VContainer`, `UniTask`, `SignalBus`, `Addressables`, and the shared UI/state-machine modules.

## Runtime Stack

| Layer | Current Choice |
|------|----------------|
| Engine | Unity `6000.3.10f1` |
| Render Pipeline | Built-in Render Pipeline |
| DI | `VContainer` |
| Async | `UniTask` |
| Messaging | `MessagePipe` via `SignalBus` |
| UI Flow | `BaseView` + `BaseScreenPresenter<TView>` |
| Scene / Asset Loading | `Addressables` via `IGameAssets` |
| Config/Data | BlueprintFlow |

## Ownership Boundaries

```text
Assets/Scripts/                         Local game layer
Assets/Submodules/GameFoundationCore/   Shared gameplay/application framework
Assets/Submodules/UITemplate/           Shared UI/state-machine framework
Assets/Submodules/Extensions/           Shared utility extensions
Assets/Submodules/Logging/              Shared logging utilities
```

Rule of thumb:

- Default feature work goes into `Assets/Scripts/`.
- Change `Assets/Submodules/` only when intentionally changing shared framework behavior.
- Vendor/plugin code is not part of the local game architecture contract.

## Assembly Layout

```text
HyperCasualGame.Scripts
|- local scenes and scopes
`- local state machine and states

GameFoundationCore.*
|- AssetLibrary
|- BlueprintFlow
|- DI
|- Signals
|- UIModule
`- Utilities

UITemplate.Scripts
`- state-machine base + local data helpers
```

## Dependency Injection Model

### Root Scope

`GameLifetimeScope` is the root `LifetimeScope`.

It currently registers:

- `RegisterGameFoundation(this.transform)`
- `RegisterUITemplate()`

This gives the project shared services such as:

- `SignalBus`
- `IGameAssets`
- `IScreenManager`
- `SceneDirector`
- `ObjectPoolManager`
- `AudioService`
- `HandleLocalUserDataServices`
- BlueprintFlow services

### Scene Scopes

```text
GameLifetimeScope
|- LoadingSceneScope
`- MainSceneScope
```

- `LoadingSceneScope` manually initializes `LoadingScreenPresenter`.
- `MainSceneScope` registers `GameStateMachine` and injects all reflected `IGameState` implementations.

## Bootstrap Flow

### Scene Entry

- `0.LoadingScene` is the startup scene in build settings.
- `1.MainScene` exists as an Addressable scene and is loaded at runtime.

### Boot Sequence

```text
0.LoadingScene
  -> GameLifetimeScope
  -> LoadingSceneScope
  -> ManualInitScreenSignal
  -> ScreenManager binds LoadingScreenPresenter to LoadingScreenView in scene
  -> LoadingScreenPresenter.BindData()
  -> UserDataManager.LoadUserData()
  -> IGameAssets.LoadSceneAsync("1.MainScene")
  -> MainSceneScope
  -> GameStateMachine.Initialize()
  -> GameHomeState.Enter()
```

## UI / Screen Architecture

### Standard Screen Pattern

- View inherits `BaseView`.
- Presenter inherits `BaseScreenPresenter<TView>` or `BaseScreenPresenter<TView, TModel>`.
- Presenter uses `[ScreenInfo(nameof(MyScreenView))]`.
- `ScreenManager` instantiates the presenter from DI, loads the prefab through `IGameAssets`, then binds the view.

### Scene-Embedded Screen Pattern

The loading screen is not loaded as a normal addressable screen.

Instead:

- the view already exists in the scene hierarchy,
- `LoadingSceneScope` calls `InitScreenManually<LoadingScreenPresenter>()`,
- `ScreenManager.OnManualInitScreen()` finds the view under `RootUICanvas.RootUIShowTransform` by the `ScreenInfo` path,
- then optionally calls `BindData()`.

This is the current pattern for bootstrap/loading screens.

### `RootUICanvas` Contract

`ScreenManager` discovers `RootUICanvas` with `FindObjectOfType<RootUICanvas>()`.

Scenes using the screen flow must provide a valid `RootUICanvas`.

Its child/root references should be configured when the scene needs separate show/hidden/overlay roots:

- `RootUIShowTransform`
- `RootUIClosedTransform`
- `RootUIOverlayTransform`

If those references are missing, `RootUICanvas` falls back to its own transform. If `RootUICanvas` itself is missing, screen binding/opening fails at runtime.

## State Machine Architecture

- Local states implement `IGameState`.
- `MainSceneScope` finds all `IGameState` types through reflection and instantiates them via the container.
- `GameStateMachine` inherits the shared `StateMachine` base.
- `GameStateMachine.Initialize()` transitions directly to `GameHomeState`.
- The shared base emits `OnStateEnterSignal` and `OnStateExitSignal` during transitions.
- Reflection discovery currently scans loaded assemblies, so new matching types in submodules/packages can also be discovered unless the registration strategy changes.

## Messaging / Signals

`SignalBus` is the project event bus abstraction over `MessagePipe`.

Current usage pattern:

1. Define a signal class.
2. Register it with `builder.DeclareSignal<TSignal>()`.
3. Subscribe in the owner lifecycle.
4. Fire from the owner that owns the state change.
5. Unsubscribe during disposal/cleanup.

Current signal groups include:

- application lifecycle signals,
- screen flow signals,
- blueprint loading signals,
- user data loaded signal.

## Addressables / Scene Loading

`IGameAssets` is the project wrapper for Addressables. It provides:

- scene loading and unloading,
- asset loading,
- preload helpers,
- cache tracking,
- scene-based auto-unload tracking.

Current scene bootstrap uses `IGameAssets.LoadSceneAsync()` directly from `LoadingScreenPresenter`.

`SceneDirector` also exists as the higher-level scene-loading service that fires loading signals and tracks `CurrentSceneName`. Use it when scene transitions need those contracts, cleanup hooks, or loading flow orchestration.

## Data / Persistence Flow

- UITemplate registers `ILocalData` implementations through reflection.
- `UserDataManager` loads and saves those models.
- `HandleLocalUserDataServices` provides the current storage implementation.
- `LoadingScreenPresenter` loads user data before entering the main scene.
- Like state discovery, local data discovery is reflection-based and should be reviewed when adding matching types outside the local game layer.

## Documentation Boundaries

- `docs/codebase-summary.md` = current-state onboarding and repo map.
- `docs/system-architecture.md` = current runtime truth.
- `docs/code-standards.md` = enforceable coding rules.
- `docs/architecture-design.md` = target-state architecture, not current runtime truth.
- `plans/20260407-mechangelion-rebuild/PLAN.md` = implementation roadmap, not runtime source of truth.
