Согласен с твоей оценкой: **маршрут не должен жить внутри `NpcAgent`/task как ручной `UniTask`-цикл**, и **`WorldLocation` не должен быть примитивом хождения**. `WorldLocation` — это семантическая точка мира: “таверна”, “рынок”, “городские ворота”. А ходить NPC должен по **waypoint/route graph**, где есть конкретные точки входа, остановки, радиусы, ожидания, направления и правила выбора.

Я бы рефакторил так:

```text id="821u28"
NpcTask
  -> RoutePlan
  -> RouteFollower
  -> LocomotionService
  -> NavMeshAgent adapter
```

А не так:

```text id="0z0mdn"
NpcTask / NpcAgent
  -> WorldLocation
  -> UniTask movement loop
  -> NavMeshAgent
```

В текущем коде уже видна причина проблемы: `NpcTravelController` одновременно хранит маршрут, индекс маршрута, callback прибытия, управляет patrol и напрямую вызывает motor; progression маршрута находится в `Update()` и завязан на `_motor.HasArrived`. ([GitHub][1]) `NpcStateController` дополнительно смешивает state transition, dialogue, travel, patrol и manual rotation. ([GitHub][2]) Это лучше разрезать на отдельные plain services.

---

# Главная архитектурная правка

Я бы ввёл **отдельную подсистему маршрутов**, независимую от NPC task scheduler.

```text id="no5vow"
Character/Npc/Tasks
    MoveToWaypointTask
    FollowRouteTask

World/Navigation
    WaypointId
    RouteId
    WaypointNode
    RouteDefinition
    RoutePlan
    RoutePlanner
    RouteRegistry

Character/Npc/Movement
    NpcLocomotionService
    NpcRouteFollower
    NpcMovementAdapter
```

`NpcTask` не должен знать, что маршрут состоит из `Transform[]`, `WorldLocation[]` или `Vector3[]`.

Он должен получить уже готовый `RoutePlan`:

```csharp id="nw1gfl"
public sealed class FollowRouteNpcTask : INpcTask
{
    private readonly RoutePlan _plan;
    private RouteProgress _progress;

    public int Priority => NpcTaskPriority.Travel;
    public NpcTaskChannel Channel => NpcTaskChannel.Locomotion;

    public bool CanSuspend => true;
    public bool CanCancel => true;

    public async UniTask<NpcTaskResult> ExecuteAsync(
        INpcTaskContext context,
        NpcTaskExecutionHandle handle,
        CancellationToken ct)
    {
        RouteFollowResult result = await context.RouteFollower.FollowAsync(
            _plan,
            _progress,
            ct);

        _progress = result.Progress;

        return result.Status switch
        {
            RouteFollowStatus.Completed => NpcTaskResult.Completed,
            RouteFollowStatus.Suspended => NpcTaskResult.Suspended,
            RouteFollowStatus.Cancelled => NpcTaskResult.Cancelled,
            _ => NpcTaskResult.Failed
        };
    }
}
```

То есть задача говорит: **“выполни этот route plan”**.
А как идти по точкам — решает `NpcRouteFollower`.

---

# Почему `WorldLocation` не должен быть маршрутом

`WorldLocation` лучше оставить как semantic layer:

```text id="mjem3g"
WorldLocationId.Tavern
WorldLocationId.Market
WorldLocationId.CastleGate
```

Но путь должен строиться через waypoint layer:

```text id="xgw6bp"
Tavern
  -> entry waypoint: tavern.entry.front
  -> inside waypoint: tavern.main_hall
  -> idle spots: tavern.table_01, tavern.bar_02
```

Иначе у тебя смешиваются две разные модели:

```text id="o8j6ds"
WorldLocation = где это в мире с точки зрения дизайна/квестов/диалогов
Waypoint = куда физически поставить NavMesh destination
Route = как пройти через набор waypoints
```

Правильная зависимость такая:

```text id="5mx8dw"
WorldLocation -> LocationAnchor/WaypointId -> RoutePlanner -> RoutePlan
```

А не:

```text id="bktosp"
NPC walks WorldLocation[]
```

---

# Предлагаемая модель данных

## WaypointId

```csharp id="1d5g2i"
public readonly record struct WaypointId(string Value);
public readonly record struct RouteId(string Value);
public readonly record struct WorldLocationId(string Value);
```

Не используй `string` везде напрямую. Typed IDs сильно уменьшают хаос.

---

## WaypointDefinition

Это runtime/domain model, не обязательно `MonoBehaviour`.

```csharp id="ou1ywf"
public sealed class WaypointDefinition
{
    public WaypointId Id { get; }
    public Vector3 Position { get; }
    public float ArrivalRadius { get; }
    public IReadOnlyList<WaypointId> Links { get; }

    public WaypointDefinition(
        WaypointId id,
        Vector3 position,
        float arrivalRadius,
        IReadOnlyList<WaypointId> links)
    {
        Id = id;
        Position = position;
        ArrivalRadius = arrivalRadius;
        Links = links;
    }
}
```

---

## RouteDefinition

Это authoring/static route.

```csharp id="1s9oz9"
public sealed class RouteDefinition
{
    public RouteId Id { get; }
    public IReadOnlyList<RouteStepDefinition> Steps { get; }
    public RouteLoopMode LoopMode { get; }

    public RouteDefinition(
        RouteId id,
        IReadOnlyList<RouteStepDefinition> steps,
        RouteLoopMode loopMode)
    {
        Id = id;
        Steps = steps;
        LoopMode = loopMode;
    }
}
```

```csharp id="53jm1h"
public sealed class RouteStepDefinition
{
    public WaypointId WaypointId { get; }
    public float WaitSeconds { get; }
    public bool FaceNextWaypoint { get; }

    public RouteStepDefinition(
        WaypointId waypointId,
        float waitSeconds = 0f,
        bool faceNextWaypoint = true)
    {
        WaypointId = waypointId;
        WaitSeconds = waitSeconds;
        FaceNextWaypoint = faceNextWaypoint;
    }
}
```

```csharp id="myw6sf"
public enum RouteLoopMode
{
    Once,
    Loop,
    PingPong
}
```

---

## RoutePlan

`RouteDefinition` — что дизайнер задал.
`RoutePlan` — что конкретный NPC будет исполнять прямо сейчас.

```csharp id="b4ma24"
public sealed class RoutePlan
{
    public RouteId? SourceRouteId { get; }
    public IReadOnlyList<RouteStep> Steps { get; }

    public RoutePlan(RouteId? sourceRouteId, IReadOnlyList<RouteStep> steps)
    {
        SourceRouteId = sourceRouteId;
        Steps = steps;
    }
}
```

```csharp id="1056kn"
public readonly struct RouteStep
{
    public WaypointId WaypointId { get; }
    public Vector3 Position { get; }
    public float ArrivalRadius { get; }
    public float WaitSeconds { get; }

    public RouteStep(
        WaypointId waypointId,
        Vector3 position,
        float arrivalRadius,
        float waitSeconds)
    {
        WaypointId = waypointId;
        Position = position;
        ArrivalRadius = arrivalRadius;
        WaitSeconds = waitSeconds;
    }
}
```

---

## RouteProgress

Это mutable runtime state. Его задача должна сохранять при suspend.

```csharp id="cdoxhr"
public sealed class RouteProgress
{
    public int StepIndex { get; private set; }

    public RouteProgress(int stepIndex = 0)
    {
        StepIndex = stepIndex;
    }

    public void Advance()
    {
        StepIndex++;
    }

    public void Reset()
    {
        StepIndex = 0;
    }
}
```

Это лучше, чем хранить `_routeIndex` внутри `NpcAgent` или `NpcTravelController`.

---

# RouteRegistry

Отдельный сервис, который знает все waypoint и route.

```csharp id="m9q7zb"
public interface IRouteRegistry
{
    bool TryGetWaypoint(WaypointId id, out WaypointDefinition waypoint);
    bool TryGetRoute(RouteId id, out RouteDefinition route);
    bool TryGetLocationAnchor(WorldLocationId id, out WaypointId waypointId);
}
```

В Unity authoring можно сделать через scene components:

```csharp id="tqkz5b"
public sealed class WaypointNode : MonoBehaviour
{
    [SerializeField] private string _id;
    [SerializeField] private float _arrivalRadius = 0.35f;
    [SerializeField] private List<WaypointNode> _links;

    public WaypointId Id => new(_id);
    public Vector3 Position => transform.position;
    public float ArrivalRadius => _arrivalRadius;

    public IReadOnlyList<WaypointNode> Links => _links;
}
```

Но это только authoring adapter. В task/service слой `WaypointNode MonoBehaviour` не должен попадать.

---

# RoutePlanner

Планировщик строит `RoutePlan`.

```csharp id="p2r08q"
public interface IRoutePlanner
{
    RoutePlan BuildRoute(RouteId routeId);
    RoutePlan BuildPath(WaypointId from, WaypointId to);
    RoutePlan BuildPathToLocation(WaypointId from, WorldLocationId locationId);
}
```

Для MVP можно начать очень просто:

```csharp id="7mljfm"
public sealed class RoutePlanner : IRoutePlanner
{
    private readonly IRouteRegistry _registry;

    public RoutePlanner(IRouteRegistry registry)
    {
        _registry = registry;
    }

    public RoutePlan BuildRoute(RouteId routeId)
    {
        if (!_registry.TryGetRoute(routeId, out RouteDefinition route))
            throw new InvalidOperationException($"Route not found: {routeId.Value}");

        var steps = new List<RouteStep>();

        foreach (RouteStepDefinition step in route.Steps)
        {
            if (!_registry.TryGetWaypoint(step.WaypointId, out WaypointDefinition waypoint))
                throw new InvalidOperationException($"Waypoint not found: {step.WaypointId.Value}");

            steps.Add(new RouteStep(
                waypoint.Id,
                waypoint.Position,
                waypoint.ArrivalRadius,
                step.WaitSeconds));
        }

        return new RoutePlan(routeId, steps);
    }

    public RoutePlan BuildPathToLocation(WaypointId from, WorldLocationId locationId)
    {
        if (!_registry.TryGetLocationAnchor(locationId, out WaypointId target))
            throw new InvalidOperationException($"Location anchor not found: {locationId.Value}");

        return BuildPath(from, target);
    }

    public RoutePlan BuildPath(WaypointId from, WaypointId to)
    {
        // MVP: direct target.
        // Later: graph search / A* over waypoint links.
        if (!_registry.TryGetWaypoint(to, out WaypointDefinition target))
            throw new InvalidOperationException($"Waypoint not found: {to.Value}");

        return new RoutePlan(
            sourceRouteId: null,
            new[]
            {
                new RouteStep(
                    target.Id,
                    target.Position,
                    target.ArrivalRadius,
                    waitSeconds: 0f)
            });
    }
}
```

На старте можно не писать полноценный A*. Но архитектурно место для него уже будет.

---

# RouteFollower

Вот сюда уходит логика “идти по точкам”.

```csharp id="fjkw1p"
public interface INpcRouteFollower
{
    UniTask<RouteFollowResult> FollowAsync(
        RoutePlan plan,
        RouteProgress progress,
        CancellationToken ct);
}
```

```csharp id="p1wnt0"
public readonly struct RouteFollowResult
{
    public RouteFollowStatus Status { get; }
    public RouteProgress Progress { get; }

    public RouteFollowResult(RouteFollowStatus status, RouteProgress progress)
    {
        Status = status;
        Progress = progress;
    }
}
```

```csharp id="qw3vk9"
public enum RouteFollowStatus
{
    Completed,
    Suspended,
    Cancelled,
    Failed
}
```

Реализация:

```csharp id="odtxsu"
public sealed class NpcRouteFollower : INpcRouteFollower
{
    private readonly INpcLocomotionService _locomotion;
    private readonly INpcRouteEvents _events;

    public NpcRouteFollower(
        INpcLocomotionService locomotion,
        INpcRouteEvents events)
    {
        _locomotion = locomotion;
        _events = events;
    }

    public async UniTask<RouteFollowResult> FollowAsync(
        RoutePlan plan,
        RouteProgress progress,
        CancellationToken ct)
    {
        try
        {
            while (progress.StepIndex < plan.Steps.Count)
            {
                RouteStep step = plan.Steps[progress.StepIndex];

                _events.PublishStepStarted(step.WaypointId, progress.StepIndex);

                await _locomotion.MoveToAsync(
                    step.Position,
                    step.ArrivalRadius,
                    ct);

                _events.PublishWaypointReached(step.WaypointId, progress.StepIndex);

                if (step.WaitSeconds > 0f)
                    await UniTask.Delay(
                        TimeSpan.FromSeconds(step.WaitSeconds),
                        cancellationToken: ct);

                progress.Advance();
            }

            _events.PublishRouteCompleted(plan.SourceRouteId);

            return new RouteFollowResult(
                RouteFollowStatus.Completed,
                progress);
        }
        catch (OperationCanceledException)
        {
            await _locomotion.PauseAsync(CancellationToken.None);

            return new RouteFollowResult(
                RouteFollowStatus.Suspended,
                progress);
        }
    }
}
```

Ключевой момент: **`RouteFollower` может использовать UniTask**, но `NpcAgent` не должен содержать кастомную логику маршрута. `NpcTask` тоже не должен вручную крутить waypoint loop.

---

# LocomotionService

Это тонкий сервис над Unity adapter.

```csharp id="dsj8es"
public interface INpcLocomotionService
{
    UniTask MoveToAsync(
        Vector3 destination,
        float arrivalRadius,
        CancellationToken ct);

    UniTask PauseAsync(CancellationToken ct);
    UniTask StopAsync(CancellationToken ct);
}
```

`NpcMotor` сейчас уже является adapter над `NavMeshAgent`: он делает `SetDestination`, проверяет `remainingDistance`, `velocity`, `pathPending`, `stoppingDistance`. ([GitHub][3]) Его можно оставить, но переименовать по роли:

```text id="ytzv6l"
NpcMotor MonoBehaviour
    -> NpcMovementAdapter
```

Plain service:

```csharp id="yi5kft"
public sealed class NpcLocomotionService : INpcLocomotionService
{
    private readonly INpcMovementAdapter _adapter;

    public NpcLocomotionService(INpcMovementAdapter adapter)
    {
        _adapter = adapter;
    }

    public async UniTask MoveToAsync(
        Vector3 destination,
        float arrivalRadius,
        CancellationToken ct)
    {
        _adapter.MoveTo(destination);

        await _adapter.WaitUntilArrivedAsync(
            destination,
            arrivalRadius,
            ct);
    }

    public UniTask PauseAsync(CancellationToken ct)
    {
        _adapter.Pause();
        return UniTask.CompletedTask;
    }

    public UniTask StopAsync(CancellationToken ct)
    {
        _adapter.Stop();
        return UniTask.CompletedTask;
    }
}
```

Adapter interface:

```csharp id="ep6uxt"
public interface INpcMovementAdapter
{
    Vector3 Position { get; }

    void MoveTo(Vector3 destination);
    void Pause();
    void Resume();
    void Stop();

    UniTask WaitUntilArrivedAsync(
        Vector3 destination,
        float arrivalRadius,
        CancellationToken ct);
}
```

Да, внутри adapter может быть `UniTask.WaitUntil`, polling или event bridge. Но это уже **Unity boundary**, а не доменная логика маршрутов.

---

# Что делать с `NpcAgent`

Я бы не делал `NpcAgent` местом, где живёт route execution.

Правильные варианты:

## Вариант A — убрать `NpcAgent`

Если у тебя уже есть scoped VContainer-сервисы на NPC, `NpcAgent` может вообще не существовать как логический класс.

Останутся:

```text id="njvzjw"
NpcTaskScheduler
NpcRuntimeState
NpcLocomotionService
NpcRouteFollower
NpcDialogueEndpoint
```

## Вариант B — оставить как facade

Если тебе удобно иметь единый entry point:

```csharp id="njepug"
public interface INpcAgent
{
    NpcId Id { get; }

    void Submit(INpcTaskRequest request);
}
```

Но внутри он не должен делать:

```text id="0ldkr8"
await MoveAlongWorldLocations(...)
await MoveAlongRoute(...)
```

Он только прокидывает task в scheduler.

---

# Как теперь выглядит команда “иди в локацию”

Публичный API:

```csharp id="vvskt9"
public interface INpcCommandService
{
    void MoveToLocation(NpcId npcId, WorldLocationId locationId);
    void FollowRoute(NpcId npcId, RouteId routeId);
    void MoveToWaypoint(NpcId npcId, WaypointId waypointId);
}
```

Реализация:

```csharp id="e9yu00"
public sealed class NpcCommandService : INpcCommandService
{
    private readonly INpcRegistry _npcRegistry;
    private readonly IRoutePlanner _routePlanner;

    public NpcCommandService(
        INpcRegistry npcRegistry,
        IRoutePlanner routePlanner)
    {
        _npcRegistry = npcRegistry;
        _routePlanner = routePlanner;
    }

    public void MoveToLocation(NpcId npcId, WorldLocationId locationId)
    {
        NpcRuntime npc = _npcRegistry.Get(npcId);

        WaypointId currentWaypoint = npc.Navigation.CurrentNearestWaypoint;

        RoutePlan plan = _routePlanner.BuildPathToLocation(
            currentWaypoint,
            locationId);

        npc.TaskScheduler.Submit(
            new FollowRouteTaskRequest(plan));
    }

    public void FollowRoute(NpcId npcId, RouteId routeId)
    {
        NpcRuntime npc = _npcRegistry.Get(npcId);

        RoutePlan plan = _routePlanner.BuildRoute(routeId);

        npc.TaskScheduler.Submit(
            new FollowRouteTaskRequest(plan));
    }

    public void MoveToWaypoint(NpcId npcId, WaypointId waypointId)
    {
        NpcRuntime npc = _npcRegistry.Get(npcId);

        RoutePlan plan = _routePlanner.BuildPath(
            npc.Navigation.CurrentNearestWaypoint,
            waypointId);

        npc.TaskScheduler.Submit(
            new FollowRouteTaskRequest(plan));
    }
}
```

Теперь `WorldLocation` не исчезает, но становится тем, чем должен быть: **семантическим alias на navigation anchor**.

---

# Как авторить waypoints в Unity

Я бы сделал три authoring-компонента:

```text id="a9gd8v"
WaypointNode
RouteAuthoring
WorldLocationAnchor
```

## WaypointNode

```csharp id="c568e6"
public sealed class WaypointNode : MonoBehaviour
{
    [SerializeField] private string _id;
    [SerializeField] private float _arrivalRadius = 0.35f;
    [SerializeField] private List<WaypointNode> _links = new();

    public WaypointId Id => new(_id);
    public Vector3 Position => transform.position;
    public float ArrivalRadius => _arrivalRadius;
    public IReadOnlyList<WaypointNode> Links => _links;
}
```

## RouteAuthoring

```csharp id="ntgxtr"
public sealed class RouteAuthoring : MonoBehaviour
{
    [SerializeField] private string _routeId;
    [SerializeField] private RouteLoopMode _loopMode;
    [SerializeField] private List<RouteStepAuthoring> _steps;

    public RouteId Id => new(_routeId);
    public RouteLoopMode LoopMode => _loopMode;
    public IReadOnlyList<RouteStepAuthoring> Steps => _steps;
}
```

```csharp id="i18ixb"
[Serializable]
public sealed class RouteStepAuthoring
{
    public WaypointNode Waypoint;
    public float WaitSeconds;
    public bool FaceNextWaypoint = true;
}
```

## WorldLocationAnchor

```csharp id="g8zrj8"
public sealed class WorldLocationAnchor : MonoBehaviour
{
    [SerializeField] private string _locationId;
    [SerializeField] private WaypointNode _defaultEntrance;

    public WorldLocationId LocationId => new(_locationId);
    public WaypointId DefaultEntranceId => _defaultEntrance.Id;
}
```

---

# VContainer registration

Примерно так:

```csharp id="87pblx"
builder.Register<IRouteRegistry, RouteRegistry>(Lifetime.Scoped);
builder.Register<IRoutePlanner, RoutePlanner>(Lifetime.Scoped);

builder.Register<INpcLocomotionService, NpcLocomotionService>(Lifetime.Scoped);
builder.Register<INpcRouteFollower, NpcRouteFollower>(Lifetime.Scoped);

builder.Register<INpcTaskScheduler, NpcTaskScheduler>(Lifetime.Scoped);
builder.Register<NpcRuntimeState>(Lifetime.Scoped);
```

Unity adapters регистрируются из scene/prefab:

```csharp id="fxxk9v"
builder.RegisterComponentInHierarchy<NpcMovementAdapter>()
    .As<INpcMovementAdapter>();
```

Route authoring collector можно сделать на сцену:

```csharp id="e5jqyx"
builder.RegisterComponentInHierarchy<RouteSceneAuthoringSource>()
    .As<IRouteAuthoringSource>();
```

А `RouteRegistry` при создании читает `IRouteAuthoringSource` и строит immutable maps.

---

# Что конкретно заменить в текущей системе

## 1. Убрать route progression из `NpcAgent` / `NpcTravelController`

Сейчас progression живёт как:

```text id="0s2p4h"
_route
_routeIndex
_isTraveling
_arrived callback
Update -> if HasArrived -> next route point
```

Это находится в `NpcTravelController`. ([GitHub][1]) В новой схеме это становится:

```text id="1lwod5"
RouteProgress
NpcRouteFollower.FollowAsync()
RouteFollowResult
```

## 2. Убрать `Action onArrived`

Callback-style плохо сочетается с task scheduler.

Вместо:

```csharp id="f2fhzc"
TravelRoute(route, () => EnterPatrol());
```

лучше:

```csharp id="x5h102"
RouteFollowResult result = await _routeFollower.FollowAsync(plan, progress, ct);
```

А task scheduler сам решает, что запускать после завершения.

## 3. Убрать связь travel -> patrol

В текущем `NpcTravelController.TravelTo/TravelRoute` отключает patrol напрямую. ([GitHub][1]) Это должен делать не movement layer. Patrol — это task/behavior, а не ответственность route follower.

Правильно:

```text id="3d2a72"
Scheduler:
    PatrolTask suspended
    FollowRouteTask active

RouteFollower:
    идёт по RoutePlan

Locomotion:
    двигает agent
```

## 4. Разделить Stop/Pause/Cancel

Для task preemption критично:

```text id="uzrjll"
Pause = сохранить путь/состояние, можно продолжить
Cancel = забыть маршрут
Stop = физически остановить agent
```

У `NpcMotor.Stop()` сейчас физически останавливает и очищает path через `ResetPath()` в старой версии кода. Это нормально для cancel, но не для suspend/resume. ([GitHub][3])

---

# Как это будет работать в твоих сценариях

## Диалог прерывает маршрут

```text id="fhfxvc"
FollowRouteTask active
  progress.StepIndex = 2

DialogueTask приходит с priority выше

Scheduler:
  cancel token для FollowRouteTask с reason Suspended

RouteFollower:
  ловит cancellation
  Pause locomotion
  возвращает Suspended с progress.StepIndex = 2

DialogueTask:
  выполняется

Scheduler:
  снова выбирает FollowRouteTask

RouteFollower:
  продолжает с progress.StepIndex = 2
```

## Новая команда маршрута заменяет старую

Здесь нужен `channel = Locomotion`.

```csharp id="0rp7cp"
public enum NpcTaskChannel
{
    Locomotion,
    Interaction,
    Emergency,
    Ambient
}
```

Правило:

```text id="sr7f7t"
В одном channel может быть только одна pending/active task.
Новый FollowRouteTask заменяет старый FollowRouteTask.
DialogueTask не заменяет FollowRouteTask, потому что другой channel.
```

То есть:

```text id="78460t"
MoveTo(Tavern)
MoveTo(Market)
```

не должны складываться в очередь. `Market` заменяет `Tavern`.

Но:

```text id="9k7wod"
MoveTo(Tavern)
Dialogue
```

не заменяют друг друга. Dialogue временно прерывает locomotion.

---

# Итоговая структура

Я бы целился вот в такую:

```text id="ox62vo"
NpcCommandService
    MoveToLocation(npc, location)
    FollowRoute(npc, route)
    MoveToWaypoint(npc, waypoint)

NpcTaskScheduler
    priority/channel/preemption

FollowRouteNpcTask
    owns RoutePlan + RouteProgress

NpcRouteFollower
    follows RoutePlan step by step

NpcLocomotionService
    MoveToAsync(Vector3, radius, ct)

NpcMovementAdapter
    wraps NavMeshAgent / Unity object

RoutePlanner
    WorldLocation/Waypoint/Route -> RoutePlan

RouteRegistry
    stores waypoints, routes, location anchors
```

Моя рекомендация: **не делай `WorldLocation` маршрутом вообще**. Сделай `WorldLocation` входом в planner, а planner пусть возвращает `RoutePlan` из waypoint’ов. Тогда NPC task system останется чистой, movement будет тестируемым, а маршруты можно будет нормально авторить, расширять и дебажить.

[1]: https://github.com/apustovitovsky/rpg-microgame/blob/main/Assets/Etheria/Scripts/Features/Character/Npc/NpcTravelController.cs "rpg-microgame/Assets/Etheria/Scripts/Features/Character/Npc/NpcTravelController.cs at main · apustovitovsky/rpg-microgame · GitHub"
[2]: https://github.com/apustovitovsky/rpg-microgame/blob/main/Assets/Etheria/Scripts/Features/Character/Npc/NpcStateController.cs "rpg-microgame/Assets/Etheria/Scripts/Features/Character/Npc/NpcStateController.cs at main · apustovitovsky/rpg-microgame · GitHub"
[3]: https://github.com/apustovitovsky/rpg-microgame/blob/main/Assets/Etheria/Scripts/Features/Character/Npc/NpcMotor.cs "rpg-microgame/Assets/Etheria/Scripts/Features/Character/Npc/NpcMotor.cs at main · apustovitovsky/rpg-microgame · GitHub"
