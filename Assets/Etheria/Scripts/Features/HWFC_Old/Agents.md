## HWFC Feature Requirements

- HWFC implementation **must** use Unity Jobs (`IJob`/`IJobParallelFor` or equivalent Jobs API) in the core generation pipeline.
- Jobs usage is mandatory and treated as a blocking acceptance criterion for the feature.
- v1 scope is **immediately Hierarchical WFC (HWFC)**, not flat WFC.
- Tile authoring tooling must be available from the Editor as a button-driven workflow similar to the first reference.
- Final map generation must run at runtime.
- Output format and generation flow should follow the first reference implementation style.
- Target dimensionality is **3D**.
- Contradiction handling strategy is **backtracking** (aligned with the second reference).
- Debug/trace visualization is required for generation state and diagnostics.
- Minimum required test coverage is smoke tests.
- The feature is isolated by its own `asmdef`; any changes to assembly definitions are allowed only with explicit user approval.
- Prefer `WFC-Classic` naming where possible to keep the code mentally aligned with the reference implementation.
- Follow .NET code style rules: https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/

## Reference Implementations

- `WFC-Classic`: https://github.com/marian42/wavefunctioncollapse (examples/wavefunctioncollapse-master)
- `HWFC-3D`: https://github.com/ria8651/hierarchical-wfc/tree/3d (examples/hierarchical-wfc-3d)

## Open Decisions (TBD)

- Performance targets are not fixed yet (generation time budget, max grid size).
- Determinism requirements across runs/platforms are not fixed yet.
- Tile rule encoding format is not fixed yet (socket-based, adjacency lists, or hybrid).
- Rotation/reflection support policy is not fixed yet.
