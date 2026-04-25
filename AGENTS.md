# AGENTS.md

## Project Focus

This repository contains several Unity samples and support packages, but the active product target is `Assets/RPG`.

Prefer changing RPG-owned code and assets instead of adapting the project around legacy FPS sample behavior.

## Architecture Map

Follow the existing dependency direction:

- `Features` may depend on `Core`
- `Core` should stay generic and should not take dependencies on gameplay-specific systems
- prefer interfaces at boundaries (`ISceneLoadingService`, `IActorFactory`, `IPickupCollectionService`, etc.)
- keep scene bootstrapping and service wiring in installer/config objects, not in unrelated gameplay classes

The project already leans on:

- `ScriptableObject` configs/installers for composition
- `VContainer` for DI and startup orchestration
- `UniTask` for async scene/load flows
- focused services instead of large god objects

`VContainer` is not incidental in this project. The architecture should be chosen and evolved using VContainer best practices as a primary constraint, not as an afterthought layered on top of ad hoc Unity code.
`UniTask` is also a core architectural choice. Treat it as the default async abstraction for gameplay and runtime flow.

## VContainer Guidelines

- treat each `LifetimeScope` as a composition root and keep registrations centralized there
- prefer plain C# services, controllers, and presenters registered through VContainer over `MonoBehaviour` orchestration
- prefer `RegisterEntryPoint` for startup flow and loop-driven app logic instead of putting control flow into random scene behaviours
- default to `Lifetime.Singleton` for registrations; use `Lifetime.Scoped` only when a dependency truly needs scope-local state or a scope-local lifetime
- default `RegisterEntryPoint` registrations to `Lifetime.Singleton` unless there is a clear, intentional reason to run them separately per child scope
- prefer constructor injection for normal C# classes
- prefer constructor injection for services
- use method injection with `[Inject]` for `MonoBehaviour` when Unity owns object creation
- use `RegisterComponentInHierarchy` for scene-owned components and `RegisterComponent` for explicitly provided existing instances
- prefer interface-based registrations and consumption at feature boundaries
- keep scene and feature composition explicit through parent-child `LifetimeScope` relationships
- avoid service locator style code, scattered `Resolve` calls, and hidden container access in gameplay logic
- avoid direct container resolution in domain/gameplay code unless there is a very strong integration reason
- when instantiating runtime objects, prefer VContainer-friendly creation/injection paths over manual wiring
- design features so that installers define wiring, services define behavior, and `MonoBehaviour` classes act mainly as views, adapters, or scene hooks
- prefer entry-point-driven flow over `MonoBehaviour.Start` as the home for application orchestration

## UniTask Guidelines

- prefer `UniTask` and `UniTask<T>` over `Task`, `ValueTask`, or coroutines for gameplay/runtime async flows
- propagate `CancellationToken` through async call chains and pass it as the last parameter
- bind async lifetime to Unity/VContainer lifetime where appropriate
- prefer explicit async orchestration in services and VContainer entry points over coroutine-driven control flow
- use Unity/PlayerLoop-aware UniTask APIs for delays, scene loading, waits, and frame sequencing
- only use `Task` when required for true .NET interop or background work that cannot stay in the UniTask model

## Source Of Truth And References

When choosing or refining architecture, consult:

- `docs/lyra-sample/**` as a high-value architectural reference based on the UE5 Lyra sample game
- official `VContainer` best practices and documentation for DI, scoping, entry points, and registration patterns

Use Lyra primarily for:

- feature/module decomposition
- separation between core systems and feature layers
- gameplay ability or interaction patterns worth adapting into Unity-friendly forms
- naming and responsibility boundaries for larger gameplay systems

Use VContainer documentation primarily for:

- composition root design
- scope ownership across root scene, additive scenes, and feature scopes
- deciding whether logic belongs in an entry point, service, installer, or `MonoBehaviour`
- choosing registration patterns such as `Register`, `RegisterEntryPoint`, `RegisterComponent`, and `RegisterComponentInHierarchy`
- keeping dependency flow explicit and container-friendly

Use UniTask documentation primarily for:

- async flow design in gameplay and scene orchestration
- cancellation propagation patterns
- PlayerLoop-aware timing and waiting
- deciding when async code belongs in entry points, services, or Unity-facing adapters

Use these as references, not as the primary place to extend product behavior:

- `Assets/FPS/**`: useful baseline sample code and original microgame behavior
- `docs/lyra-sample/**`: design/reference material, not active Unity runtime code

Avoid editing third-party/vendor areas unless the task is explicitly about them:

- `Assets/GAS/**`
- `Assets/FPS/**`
- package-managed dependencies in `Packages/`

If product logic currently mirrors something in `FPS`, prefer porting the idea into `RPG` abstractions instead of coupling new RPG code directly to FPS sample classes.
If there is a choice between ad hoc local structure and a cleaner architecture already demonstrated in Lyra, prefer the Lyra-shaped design adapted to Unity and the current RPG assembly boundaries.
If there is a choice between a quick Unity-style shortcut and a cleaner VContainer-native composition pattern, prefer the VContainer-native design.
If there is a choice between coroutine-first code and a clean UniTask-based async flow, prefer UniTask unless the task explicitly requires a coroutine integration point.

## Testing And Verification

Do not write new tests by default.
Unless the task explicitly asks for tests, focus on implementation and manual verification instead of adding or updating automated coverage.


## Code Style

Follow the existing RPG C# style:

- keep classes small and single-purpose
- depend on interfaces at feature boundaries
- use `sealed` for concrete service classes when extension is not intended
- keep `MonoBehaviour` classes thin; move reusable logic into plain C# services where practical
- keep `ScriptableObject` types focused on configuration/composition, not runtime stateful orchestration
- preserve current namespace style such as `RPG.Core` and `RPG.Gameplay`
- use clear, descriptive names over abbreviations
- avoid introducing reflection-heavy or magic auto-wiring patterns when explicit installers already solve the problem
- prefer `UniTask` return types for project async APIs
- thread cancellation tokens through async methods instead of hiding cancellation internally
- avoid mixing coroutines and `Task`-based flows into gameplay code when `UniTask` can express the same behavior more clearly

There is also a naming/style reference in `docs/Use_a_C__style_guide_for_clean_and_scalable_game_code_Unity_6_edition_e-book.txt`.
Use it when naming new types, members, and gameplay concepts.

Repository-level editor settings are minimal. `.editorconfig` currently only disables namespace-folder enforcement, so match the surrounding file style rather than inventing a new one.

## Unity Asset Safety

Be careful with Unity-owned files:

- never hand-edit `Library`, `Temp`, or `Logs`
- keep `.meta` files together with moved/renamed assets
- do not casually regenerate or delete prefabs, scenes, or installer assets
- avoid modifying unrelated serialized references in `.unity`, `.prefab`, or `.asset` files
- if a change touches scenes or prefabs, mention that in the summary because diffs are hard to inspect

The working tree may already contain in-progress RPG asset changes. Do not revert or overwrite unrelated changes unless explicitly asked.

## Working Agreement For Agents

When helping in this repository:

- assume `RPG` is the target experience unless the task says otherwise
- prefer incremental changes that preserve current assembly boundaries
- mention when a proposal depends on vendor/sample code versus RPG-owned code
- include manual editor verification notes for any scene, prefab, or installer change
