# Research Report: Unity C# Standards and Architecture Docs

_Conducted: 2026-04-07_

## Executive Summary
- For a Unity team using DI, async, Addressables, and message/event systems, the best standards baseline is: Microsoft C# conventions for syntax/style, Microsoft DI guidance for composition and lifetimes, Microsoft async guidance for naming/non-blocking flows, Unity Addressables docs for runtime loading and ownership, and C4 for architecture documentation shape. [1][2][3][4][5]
- The highest-value team rules to codify are: enforce style with `.editorconfig`; require constructor injection and small services; ban async work in DI registration/constructors; standardize async naming and cancellation; centralize Addressables ownership/release; and document runtime flows with C4 context/container/component + dynamic diagrams for message-driven sequences. [1][2][3][4][5]

## Sources Consulted
1. Microsoft Learn: C# coding conventions. [1]
2. Microsoft Learn: dependency injection guidelines. [2]
3. Microsoft Learn: async scenarios in C#. [3]
4. Unity Addressables manual 1.21. [4]
5. Official C4 model site by Simon Brown. [5]

## Recommended Conventions To Adopt

## 1. Core C# Style
- Use file-scoped namespaces, `using` directives outside namespaces, four-space indentation, Allman braces, one statement per line, and XML docs for public APIs. Enforce via `.editorconfig` + analyzers in CI. [1]
- Prefer explicit types when the RHS is not obvious; allow `var` when the type is obvious or for LINQ/task collections. [1][3]
- Require meaningful names over type-encoded names; keep comments short and separate from code lines. [1]
- Standardize static access through type names, not instances; avoid broad `catch (Exception)` unless filtered/handled. [1]

## 2. DI / Composition Rules
- Prefer constructor injection only; ban service locator patterns, static service access, and `BuildServiceProvider` during registration. [2]
- Keep services small and SRP-aligned; if a class needs many dependencies, refactor responsibilities. [2]
- Keep DI factories fast and synchronous; no async registration factories and no blocking on async during resolution. [2][3]
- Enable scope validation in development/CI; explicitly review singleton -> scoped/transient dependencies. [2]
- Only use singleton for shared or expensive state; otherwise default to transient/scoped depending on scene/lifetime boundaries. [2]

## 3. Async Rules
- Suffix asynchronous methods with `Async`; allow `async void` only for Unity/UI event handlers. [3]
- For I/O-bound work, `await` the underlying async API directly; do not wrap it in `Task.Run`. For CPU-bound work, use background execution intentionally and measure before adding threading overhead. [3]
- Ban `.Result`, `.Wait()`, and `Task.WaitAll/Any` in gameplay/runtime code; use `await`, `Task.WhenAll`, `Task.WhenAny`, and delays instead. [3]
- Require cancellation support on long-running operations that cross scene/screen/state boundaries.
- When creating task collections with LINQ, force execution with `.ToArray()`/`.ToList()` before `WhenAll`/`WhenAny`. [3]

## 4. Addressables Rules
- Treat Addressables as the single abstraction for runtime asset loading; avoid ad hoc `Resources`-style paths for production gameplay content. [4]
- Define one owner for each handle/load path: the system that loads an Addressable is responsible for release/unload policy. [4]
- Standardize group/profile naming and remote/local intent in docs; keep content catalog/build/distribution steps explicit for the team. [4]
- Document preload dependencies and scene/asset loading boundaries to avoid hidden coupling and memory regressions. [4]
- Require use of Addressables diagnostic tools during performance QA and content validation. [4]

## 5. Event / Message System Rules
- Use messages/events for cross-feature coordination, not for replacing direct method calls inside a cohesive service boundary.
- Messages should be small, immutable DTOs named by intent/past tense for facts (`PlayerDiedSignal`) or imperative for commands (`OpenScreenCommand`).
- Handlers should stay thin: validate, translate, delegate. No scene loading, asset loading, and domain mutation mixed in a single subscriber.
- Document publish/subscribe ownership in architecture docs so teams know who emits, who reacts, and whether ordering matters. [2][5]
- Avoid hidden async in subscribers; if a handler becomes asynchronous, document completion/ordering/error policy explicitly. [3][5]

## What To Put In `docs/code-standards.md`
- `Naming`: `PascalCase` for types/public members, `camelCase` for locals/parameters, `Async` suffix for async methods, suffix event payloads with `Signal`/`Event` consistently. [1][3]
- `DI`: constructor injection only; no service locator; no static container access; validate scopes. [2]
- `Async`: no blocking waits; async only where the boundary needs it; cancellation on scene/state transitions. [3]
- `Addressables`: one loader owns release; use documented keys/labels/groups; no unmanaged runtime asset references. [4]
- `Events`: immutable payloads, one intent per message, subscriber side effects kept narrow and documented.
- `Enforcement`: `.editorconfig`, analyzer warnings as errors for core style rules, PR checklist for lifetimes/async/addressable ownership. [1][2]

## What To Put In Architecture Docs
- Use C4 level 1-3 as the default set: system context, container, component. Add dynamic diagrams for scene boot, screen open, combat flow, and asset-load/message sequences. [5]
- For each container/component, document: responsibility, inbound dependencies, outbound dependencies, lifetime/scope, async behavior, and emitted/consumed messages. [2][3][5]
- Add one runtime ownership table: `Asset/Handle`, `Loaded By`, `Released By`, `Scope`, `Failure Policy`. [4]
- Add one interaction table for events/messages: `Publisher`, `Message`, `Subscribers`, `Ordering`, `Async?`, `Idempotent?`. [3][5]
- Keep diagrams notation-light and reviewable; optimize for developer comprehension, not UML completeness. [5]

## Suggested Repo Artifacts
- `.editorconfig` for C# style + analyzer severity. [1]
- `docs/code-standards.md` for mandatory rules.
- `docs/architecture/01-context.md`, `02-containers.md`, `03-components-*.md` using C4. [5]
- `docs/architecture/runtime-flows.md` with dynamic diagrams for DI boot, scene transition, async load, and message propagation. [3][4][5]
- `docs/architecture/adr/` for decisions like `Addressables over Resources`, `MessagePipe for cross-module signals`, `UniTask at Unity boundaries`.

## Team Policy Starter
```md
- Constructor injection only.
- No `.Result` / `.Wait()` in runtime code.
- Async methods end with `Async`.
- The loader owns the Addressables release.
- One message = one intent; payloads are immutable.
- Every new subsystem adds/updates a C4 component doc and one runtime flow.
```

## Bottom Line
- If you adopt only six rules, make them these: `.editorconfig` enforcement, constructor injection only, no blocking async, explicit Addressables ownership, immutable narrow messages, and mandatory C4 component + dynamic docs for new systems. Those six give the biggest consistency win with the least process overhead. [1][2][3][4][5]

## References
- [1] Microsoft Learn, "C# coding conventions," updated 2025-01-15. https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions
- [2] Microsoft Learn, "Dependency injection guidelines," updated 2026-01-14. https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection/guidelines
- [3] Microsoft Learn, "Asynchronous programming scenarios," updated 2025-03-12. https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/async-scenarios
- [4] Unity Technologies, "Addressables package 1.21.21 manual." https://docs.unity3d.com/Packages/com.unity.addressables@1.21/manual/index.html
- [5] Simon Brown, "The C4 model for visualising software architecture" (official site). https://c4model.com/