## HWFC Implementation Manifest

Goal: implement a **classic multipass hierarchical WFC**, not a DAG-based tileset compiler.

### Core Direction

- Use a mature `WFC-Classic` solver as the base algorithm.
- Hierarchy is implemented through **multiple generation passes**, not through semantic DAG inheritance.
- Each pass produces constraints for the next pass.

## HWFC Feature Requirements

- The feature is isolated by its own `asmdef`; any changes to assembly definitions are allowed only with explicit user approval.
- Prefer `WFC-Classic` implementation but hierarchical.
- Follow .NET code style rules: https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/

## Reference Implementations

`WFC-Classic`: github.com (path: examples/wavefunctioncollapse-master).
This repository serves as the primary reference for the code modification.

`HWFC-3D`: github.com (path: examples/hierarchical-wfc-3d).
This project is included solely for additional context and information.

## Architecture Concern: Marian-Style WFC

The Marian implementation is a good reference for a mature WFC algorithm, but its architecture should not be copied directly.

### Main Problem

The algorithm, Unity authoring data, runtime state, and scene spawning are tightly coupled.

Examples of coupling:

- `Module` depends on `ModulePrototype`
- `Module` stores `Prefab`
- `Slot` stores `GameObject`
- `Slot` knows about `AbstractMap`
- `ModuleSet` depends on global `ModuleData.Current`
- entropy calculation reads `module.Prototype.Probability`
- collapse logic directly triggers map notifications and build queues

### Why This Is Bad For HWFC

Classic multipass HWFC needs the same solver to run on different abstraction levels:

```text
Layout modules
Zone modules
Structure modules
Facade modules
Detail modules

## Обратить внимание я бы предложил на 10 вещей.

# 1. Разделить problem, state и authoring

Не смешивать:

ModuleSO / Prefab / sockets / tags

с runtime solver state:

Slot domains
ModuleHealth
RemovalQueue
History
EntropyQueue

Хорошая схема:

WfcTileset          // compiled modules + neighbor rules
WfcGraph            // slots + adjacency
WfcSolverState      // текущие domains/health/history
WfcSolver           // алгоритм
UnityBuilder        // спавн prefab-ов

Это сильно упростит детерминизм, тесты и чанки.

# 2. Не использовать координаты в propagation

Для bounded world лучше заранее собрать:

WfcNeighbor[] neighbors;

А не делать каждый раз:

GetSlot(position + direction)

Координаты пусть будут metadata. Solver должен работать по индексам.

# 3. Явно определить DirectionModel

Не хардкодить 6, (d + 3) % 6, Orientations.Direction.

Лучше:

public sealed class DirectionModel
{
    public int Count;
    public int[] Opposite;
}

Так ты потом легко сделаешь:

6 направлений
8 направлений
произвольный graph
socket-anchor graph

# 4. Сразу сделать compiled PossibleNeighbors

Не проверять сокеты во время propagation.

На bake/init этапе:

Module sockets/tags
→ PossibleNeighbors[module][direction] : ModuleSet
→ SupportedBy[module][direction] или reverse adjacency

Во время solver-а только битовые операции и счетчики.

# 5. ModuleHealth хранить в state, не в Slot-объекте

У Marian Slot сам хранит ModuleHealth. Это удобно, но связывает всё со всем.

Лучше:

ModuleSet[] domains;              // slotIndex → possible modules
short[,,] moduleHealth;           // slotIndex, direction, moduleIndex

или плоско:

short[] health;

Так проще клонировать, откатывать, профилировать.

# 6. Undo должен быть полным

Если делаешь backtracking, история должна восстанавливать всё, что изменилось:

domain changes
module health changes
collapsed state / selected module
entropy version
queue state или очередь очищается и пересобирается

Самая частая ошибка — восстановить ModuleSet, но забыть корректно восстановить ModuleHealth.

# 7. Решить: restart, Marian-style backtrack или decision stack

Уровни зрелости:

просто restart при конфликте           — легко
откат N шагов как Marian               — нормально
decision stack с remaining alternatives — лучше
conflict-directed backjumping           — сложно

Для тебя оптимально:

DecisionStack + UndoLog

То есть помнить не только “что удалили”, но и “какие альтернативы еще можно попробовать”.

# 8. Entropy queue с versioning

Не сканировать весь work area каждый раз, если мир большой.

Используй:

PriorityQueue
entry = slotIndex + entropy + version

При изменении domain:

version++
push new entry

При pop:

если entry.version != currentVersion → stale, пропустить

Это проще, чем mutable heap.

# 9. Детерминизм

Для твоей задачи это критично.

Избегать:

HashSet iteration order
Dictionary iteration order
static Random
общий Random в нескольких потоках
порядок завершения Task-ов как источник выбора

Лучше:

seed = Hash(globalSeed, layerId, chunkCoord, passId);

И все tie-break-и должны идти через deterministic RNG.

# 10. Ограничения от верхнего слоя применять до propagation

Для HWFC pipeline нижний solve должен стартовать уже с урезанными domains:

AllModules
∩ Theme
∩ ZoneType
∩ BoundaryRole
∩ RequiredDirectionMask
∩ FixedMarkers

После применения initial constraints сразу запускай propagation до пустой очереди, и только потом начинай collapse.

⸻

Главный совет: сначала сделай solver для одного bounded graph-а без Unity-объектов.

Минимальный runtime API:

public sealed class WfcProblem
{
    public WfcTileset Tileset;
    public WfcGraph Graph;
    public ModuleSet[] InitialDomains;
    public int Seed;
}

public sealed class WfcResult
{
    public int[] SelectedModules;
}

Если этот слой чистый, всё остальное — layout, чанки, замки, темы, фасады, SO, prefab spawn — будет намного проще добавлять сверху.