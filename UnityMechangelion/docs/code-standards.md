# Code Standards

## Scope

This document defines the coding rules for the local game layer in this repository.

- Default target: `Assets/Scripts/`
- Shared framework references: `Assets/Submodules/GameFoundationCore/`, `Assets/Submodules/UITemplate/`, `Assets/Submodules/Extensions/`, `Assets/Submodules/Logging/`
- Vendor/plugin code is out of scope unless explicitly being maintained.

When local code and shared framework behavior disagree, update this document to match the real project contract instead of copying generic Unity advice.

## Core Principles

- Keep local gameplay code small and explicit.
- Prefer composition over inheritance.
- Prefer DI-managed services over static/global access.
- Keep project rules practical and reviewable.
- Separate current implementation rules from future-state design ideas.

## Naming Conventions

| Type | Convention | Example |
|------|------------|---------|
| Class | `PascalCase` | `GameStateMachine` |
| Interface | `IPascalCase` | `IScreenManager` |
| Method | `PascalCase` | `OpenScreen` |
| Property | `PascalCase` | `CurrentActiveScreen` |
| Private field | `camelCase` | `screenManager` |
| Parameter | `camelCase` | `signalBus` |
| Local variable | `camelCase` | `nextScreen` |
| Constant | `PascalCase` | `MainSceneName` |
| Enum | `PascalCase` | `ScreenStatus.Opened` |

## Member Access Rule

Use `this.` for instance members.

```csharp
this.signalBus.Fire(new GameStartedSignal());
await this.screenManager.OpenScreen<HomeScreenPresenter>();
```

## Ownership and File Placement

- Put project-specific gameplay code in `Assets/Scripts/`.
- Treat `Assets/Submodules/` as shared framework code.
- Do not copy framework types into local code just to avoid references.
- Do not add new code to vendor/plugin folders.
- Keep new types in the assembly that owns the runtime behavior.

Current local structure:

```text
Assets/Scripts/
|- Scenes/
|  |- GameLifetimeScope.cs
|  |- Loading/
|  |- Main/
|  `- Screen/
|- Models/
`- StateMachines/
   `- Game/
```

If the local game layer grows, extend this structure from the current folders instead of inventing a second parallel convention.

## Dependency Injection Rules

### Hard Rules

- Use constructor injection for non-`MonoBehaviour` classes.
- Register services/states/presenters through scopes or registration extensions.
- Do not instantiate DI-managed services with `new` outside composition/setup code.
- Do not use service locators in gameplay code.

```csharp
public class MyService
{
    private readonly SignalBus signalBus;
    private readonly IScreenManager screenManager;

    public MyService(SignalBus signalBus, IScreenManager screenManager)
    {
        this.signalBus = signalBus;
        this.screenManager = screenManager;
    }
}
```

### MonoBehaviour Exception

Unity does not support constructor injection on `MonoBehaviour`.

Use method injection only when the type must remain a `MonoBehaviour`:

```csharp
public class MyBehaviour : MonoBehaviour
{
    private SignalBus signalBus;

    [Inject]
    public void Construct(SignalBus signalBus)
    {
        this.signalBus = signalBus;
    }
}
```

### Scope Rules

- Root/shared services belong in `GameLifetimeScope` or shared registration extensions.
- Scene-specific services belong in the matching `SceneScope`.
- Reflection-based state registration currently happens in `MainSceneScope` and runtime discovery scans loaded assemblies.
- Policy: prefer keeping project gameplay states in the local game assembly even though the current runtime can discover matching types from other loaded assemblies.

## Async Rules

- Use `UniTask`, not `Task`, for gameplay and framework-facing async flows.
- Do not block on async work with `.Wait()`, `.Result`, or sync wrappers.
- Keep scene/screen boot flows explicit and readable.
- Prefer returning the async operation you already own instead of wrapping it unnecessarily.

```csharp
public override async UniTask BindData()
{
    await this.userDataManager.LoadUserData();
    await this.LoadSceneAsync();
}
```

## SignalBus Rules

### Hard Rules

- Define signals as classes.
- Register each signal with `builder.DeclareSignal<TSignal>()` before use.
- Subscribe and unsubscribe from the owner lifecycle.
- Keep signal payloads focused on one state change or event.
- Use `SignalBus`, not raw `MessagePipe`, from project code.

```csharp
public class GameStartedSignal
{
}

builder.DeclareSignal<GameStartedSignal>();

this.signalBus.Subscribe<GameStartedSignal>(this.OnGameStarted);
this.signalBus.Fire(new GameStartedSignal());
this.signalBus.Unsubscribe<GameStartedSignal>(this.OnGameStarted);
```

### Review Notes

- If a new subsystem depends on a signal, verify the registration site in the same change.
- If a shared base class already fires a signal, confirm the signal is declared by the active scope/registration path.

## Screen and Presenter Rules

### Naming

| Type | Format | Example |
|------|--------|---------|
| File | `{Name}ScreenView.cs` | `LoadingScreenView.cs` |
| View class | `{Name}ScreenView` | `LoadingScreenView` |
| Presenter class | `{Name}ScreenPresenter` | `LoadingScreenPresenter` |
| Model class | `{Name}ScreenModel` | `HomeScreenModel` |
| Prefab | `{Name}ScreenView` | `HomeScreenView` |

### Structure

- `ScreenInfo` path is the runtime lookup key used by the screen system.
- By convention, keep it aligned with the view/prefab name unless there is a clear reason not to.
- Prefer colocating view + presenter in one file for small local screens.
- Add the model to the same file only when it stays small and screen-scoped.
- Split model/presenter/view into separate files when the screen grows.

```csharp
[ScreenInfo(nameof(MyScreenView))]
public class MyScreenPresenter : BaseScreenPresenter<MyScreenView>
{
}
```

### Runtime Patterns

- Use `screenManager.OpenScreen<TPresenter>()` for normal addressable screens.
- Use `InitScreenManually<TPresenter>()` only for scene-embedded/bootstrap screens.
- Any scene using screen flow must contain a valid `RootUICanvas`.

## State Machine Rules

- Local states implement `IGameState`.
- Inject dependencies through constructors.
- Keep `Enter()` and `Exit()` focused on orchestration, not object graph creation.
- If a state needs to trigger transitions, implement `IHaveStateMachine`.

```csharp
public class PlayingState : IGameState, IHaveStateMachine
{
    private readonly SignalBus signalBus;

    public IStateMachine StateMachine { get; set; }

    public PlayingState(SignalBus signalBus)
    {
        this.signalBus = signalBus;
    }

    public void Enter()
    {
        this.signalBus.Fire(new GameStartedSignal());
    }

    public void Exit()
    {
    }
}
```

## Addressables and Scene Loading Rules

- Use `IGameAssets` as the default entry point for Addressables work.
- Use `SceneDirector` when scene transitions need loading signals and cleanup orchestration.
- Avoid calling raw `Addressables.*` APIs directly from local gameplay code unless you are extending the asset wrapper itself.
- Keep scene names/keys centralized when they are reused in multiple places.
- A single bootstrap-only key is acceptable temporarily, but duplicated keys should be consolidated quickly.

```csharp
private const string MainSceneName = "1.MainScene";

protected virtual AsyncOperationHandle<SceneInstance> LoadSceneAsync()
{
    return this.gameAssets.LoadSceneAsync(MainSceneName);
}
```

## Serialized Fields and Scene Contracts

- Use `[SerializeField] private` for inspector-owned references.
- Do not hide critical scene contracts in comments only; document them in the owning architecture doc.
- If a system depends on a scene object such as `RootUICanvas`, make that dependency explicit in docs and scene setup.
- Do not move/rename runtime-critical scene objects without checking lookup code.

## Assembly Rules

- Do not create circular assembly references.
- Keep UI flow code in the existing screen/presenter architecture instead of bypassing it ad hoc.
- Put tests in dedicated test assemblies when they are added.

## Prohibited Practices

- Using `Task` in gameplay code when `UniTask` is the project standard
- Blocking async calls with `.Wait()` or `.Result`
- Direct `new` for DI-managed services in runtime code
- Static singleton patterns for new gameplay systems
- Raw `MessagePipe` usage from local feature code
- Raw `Addressables` calls from local feature code without wrapper justification
- Empty `catch` blocks
- Duplicated scene keys across multiple files

## Lightweight Review Checklist

- Does the new type live in the correct assembly/folder?
- Does DI happen through the correct scope and constructor?
- Does async code use `UniTask` end to end?
- Are signals declared, subscribed, and unsubscribed correctly?
- Does the screen/state follow the current local naming and runtime pattern?
- If scene/UI contracts changed, were the docs updated too?

## Doc Maintenance Triggers

Update docs when a change touches any of these:

- `GameLifetimeScope` or any `SceneScope`
- boot flow between `0.LoadingScene` and `1.MainScene`
- `RootUICanvas` contract
- screen/presenter loading behavior
- signal registration topology
- Addressables keys, scene groups, or wrapper usage
- assembly boundaries or major folder ownership
