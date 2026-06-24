Да, ты точно правильно видишь проблему: **NavMesh — это не система рутины**. NavMesh отвечает на узкий вопрос:

> Как из текущей позиции пройти к destination по walkable surface?

А рутина отвечает на совсем другой вопрос:

> Почему NPC туда идёт, каким способом, по какому маршруту, что делает по пути, что делать при прерывании, что делать после прибытия?

Поэтому я бы строил систему в несколько слоёв.

---

# 1. Базовая модель: Routine ≠ Movement ≠ NavMesh

Я бы разделил так:

```text id="ernggr"
NpcRoutineService
  решает, какая активность должна быть сейчас

NpcBehaviorState
  исполняет активность: работать, сидеть, патрулировать, сопровождать, ждать, говорить

NpcTravelController
  исполняет перемещение как задачу

NpcMotor / NavMeshMotor
  низкоуровнево двигает агента

NavMeshAgent
  строит путь по NavMesh к конкретной точке
```

То есть `NavMeshAgent.SetDestination()` должен быть самым нижним уровнем. В документации Unity `SetDestination` именно “sets or updates the destination thus triggering calculation for a new path”, а сам `NavMeshAgent` даёт свойства вроде `remainingDistance`, `pathStatus`, `steeringTarget`, `isStopped`, `autoBraking`, `avoidancePriority`, `areaMask`, `speed` и т.д. Это полезные механизмы, но они не описывают смысловую рутину NPC. ([docs.unity3d.com][1])

Правильная иерархия:

```text id="pjwz7n"
Routine:    08:00–12:00 WorkAtForge
Behavior:   go to forge, occupy freepoint, play work animation
Travel:     follow route or navigate to forge
Motor:      NavMeshAgent.SetDestination(nextPoint)
```

---

# 2. Gothic-like модель: Routine вызывает Script State

В Gothic очень хороший ориентир: daily routine исполняется, когда NPC “больше нечего делать”; routine-строки задают activity, временной интервал и waypoint. Пример из анализа Gothic: `TA_Sleep`, `TA_SitAround`, `TA_Smalltalk` регистрируются на конкретные интервалы времени и waypoint. ([ataulien.github.io][2])

Ключевая идея не в том, что NPC просто идёт в точку. Ключевая идея такая:

```text id="kimly9"
В заданный промежуток времени NPC входит в поведенческое состояние.
Это состояние само решает:
  куда идти,
  что искать рядом,
  какую анимацию играть,
  что делать при занятости места,
  когда завершиться.
```

В Gothic script state имеет begin/init, loop и end-фазы; begin готовит действие, loop повторяется пока состояние активно, end корректно завершает состояние. ([GothicMDK][3])

Я бы прямо скопировал эту концепцию в Unity.

---

# 3. Твоя целевая структура routine

## Routine Definition

```csharp id="76c9h7"
[CreateAssetMenu]
public sealed class NpcRoutineDefinitionSO : ScriptableObject
{
    public string RoutineId;
    public List<NpcRoutineEntry> Entries;
}

[Serializable]
public sealed class NpcRoutineEntry
{
    public int StartMinute; // 0..1439
    public int EndMinute;   // 0..1439

    public string BehaviorId;    // "work_forge", "sleep", "patrol", "guard", "smalltalk"
    public string TargetId;      // location/freepoint/route/group id
    public string RouteId;       // optional
    public RoutinePriority Priority;
}
```

Пример:

```text id="29zayw"
HakonRoutine:
  06:00–08:00  eat_breakfast      tavern_table_01
  08:00–12:00  work_forge         blacksmith_work_area
  12:00–13:00  eat_lunch          tavern_table_02
  13:00–18:00  work_forge         blacksmith_work_area
  18:00–22:00  sit_campfire       old_camp_fire_01
  22:00–06:00  sleep              hakon_bed
```

Но это не значит “телепортируйся в `blacksmith_work_area`”. Это значит:

```text id="euw1d3"
Стартуй поведенческое состояние WorkForgeState с target = blacksmith_work_area.
```

---

# 4. Behavior State — главный исполнитель рутины

Тебе нужны не просто точки маршрута, а **состояния поведения**.

```csharp id="fwmnum"
public interface INpcBehaviorState
{
    string Id { get; }

    void Enter(NpcBehaviorContext context, NpcRoutineEntry entry);
    void Tick(float deltaTime);
    void Exit();
    bool IsFinished { get; }
}
```

Примеры:

```text id="slz6jw"
SleepState
SitState
WorkAtForgeState
GuardState
PatrolState
SmalltalkState
FollowPlayerState
GuidePlayerState
FleeState
TravelToLocationState
```

Почему это важно: `Sleep`, `WorkAtForge`, `Guard`, `Patrol` и `FollowPlayer` используют перемещение, но это разные gameplay-сценарии.

---

# 5. Freepoints / Smart Objects вместо “точка назначения”

Для Gothic-like рутины очень важны не просто координаты, а **места действия**.

Тебе нужен аналог Gothic freepoint / mobsi / smart object:

```csharp id="mx54y7"
public sealed class NpcActivityPoint : MonoBehaviour
{
    public string PointId;
    public string ActivityType; // "forge", "sleep", "sit", "smalltalk", "guard"
    public Transform StandPoint;
    public Transform LookAtPoint;
    public bool IsOccupied;
    public int Priority;
}
```

Например `WorkAtForgeState` делает не:

```text id="haijqe"
go to blacksmith_work_area.position
play animation
```

А:

```text id="dn0uxm"
найти свободный ActivityPoint типа "forge" рядом с blacksmith_work_area
занять его
дойти до StandPoint
повернуться к LookAtPoint
запустить forge animation
освободить point при Exit()
```

Это решает сразу много проблем:

```text id="c41e8q"
NPC не встают в одну точку
NPC могут выбирать альтернативное рабочее место
поведение переносимо между сценами
рутину можно задавать через смысловые зоны, а не координаты
```

В Gothic-подобной модели это особенно важно: по анализу Gothic, daily routine может указывать waypoint, а state потом ищет nearby freepoint нужного типа; например smalltalk-state идёт к waypoint, затем ищет свободный freepoint `SMALLTALK` и поворачивается в нужную сторону. ([ataulien.github.io][2])

---

# 6. Route — отдельная сущность, не равная NavMesh path

Теперь к твоему главному вопросу: заранее подготовленный маршрут.

Да, тебе нужна отдельная сущность:

```csharp id="7u6s72"
[CreateAssetMenu]
public sealed class NpcRouteDefinitionSO : ScriptableObject
{
    public string RouteId;
    public List<RouteNodeRef> Nodes;
    public RouteTraversalMode TraversalMode;
}

public enum RouteTraversalMode
{
    Once,
    Loop,
    PingPong,
    Random,
    PatrolWithWaits
}

[Serializable]
public sealed class RouteNodeRef
{
    public string LocationId;
    public float WaitSeconds;
    public string OnArriveBehaviorId; // optional: look_around, inspect, sit, guard
    public MovementMode MovementMode; // Walk, Run, Sneak
}
```

Но `Route` — это **не baked path**. Это список смысловых waypoint-ов.

```text id="r9wmvr"
guard_patrol_old_gate:
  old_gate_left_post     wait 8 sec
  old_gate_center        wait 2 sec
  old_gate_right_post    wait 8 sec
  old_gate_watch_fire    wait 4 sec
```

А между этими точками `NpcTravelController` использует NavMesh.

То есть:

```text id="d3zyyu"
Prepared route:
  A -> B -> C -> D

NavMesh:
  строит физический путь от A до B,
  потом от B до C,
  потом от C до D.
```

Это нормальный подход. Не надо вручную рисовать каждый поворот маршрута, если тебе не нужна кинематографическая точность.

---

# 7. Когда нужен именно заранее заданный spline/path

Есть два разных типа маршрутов:

## A. Semantic route

Для 90% рутин:

```text id="s9iph6"
точка кузницы -> точка таверны -> точка кровати
```

Внутри каждого перехода можно использовать NavMesh.

## B. Authored path

Для случаев, где NPC должен идти именно по заданной траектории:

```text id="u5jml4"
торжественная процессия
стражник идёт по стене определённым обходом
NPC ведёт игрока красивой дорогой
NPC должен пройти через конкретные ворота
кат-сценный маршрут
узкая тропа, где NavMesh может выбрать некрасивый путь
```

Для этого я бы добавил:

```csharp id="tufmnl"
public sealed class NpcAuthoredPath : MonoBehaviour
{
    public string PathId;
    public List<Transform> Nodes;
    public PathTraversalMode Mode;
}
```

И режим движения:

```text id="hvjhbw"
FollowAuthoredPath:
  for each authored node:
      NavMeshAgent.SetDestination(node.position)
      wait until reached
```

Это всё ещё использует NavMesh между узлами, но путь контролируется дизайнером.

Если нужна абсолютно точная траектория — например кат-сцена — тогда можно временно отключать `NavMeshAgent.updatePosition/updateRotation` и вести root motion / spline follower, но для обычной рутины я бы так не делал. В Unity у `NavMeshAgent` есть свойства `updatePosition`, `updateRotation`, `nextPosition`, `velocity`, `Move`, что позволяет отделять симуляцию агента от transform/root-motion при необходимости. ([docs.unity3d.com][1])

---

# 8. NpcTravelController: центральный слой перемещения

Я бы сделал один компонент, через который проходят все “долгие перемещения” NPC:

```csharp id="uuwb2v"
public sealed class NpcTravelController : MonoBehaviour
{
    public TravelTask CurrentTask { get; private set; }

    public void TravelToLocation(string locationId, TravelOptions options);
    public void FollowRoute(string routeId, TravelOptions options);
    public void FollowTarget(Transform target, FollowOptions options);
    public void FleeFrom(Transform threat, string fallbackLocationId);
    public void Stop(TravelStopReason reason);
}
```

И тип задачи:

```csharp id="9jwjsh"
public enum TravelTaskType
{
    GoToLocation,
    FollowRoute,
    PatrolRoute,
    FollowTarget,
    GuidePlayer,
    FleeToLocation,
    WanderInArea
}
```

Важное правило:

```text id="5ks4eq"
BehaviorState не должен напрямую вызывать NavMeshAgent.SetDestination.
BehaviorState должен просить NpcTravelController выполнить travel-задачу.
```

Например `PatrolState`:

```csharp id="2tlijo"
public sealed class PatrolState : INpcBehaviorState
{
    public void Enter(NpcBehaviorContext context, NpcRoutineEntry entry)
    {
        context.Travel.FollowRoute(entry.RouteId, TravelOptions.WalkLoop);
    }

    public void Exit()
    {
        context.Travel.Stop(TravelStopReason.BehaviorChanged);
    }
}
```

---

# 9. Перемещение внутри routine: логика такая

Допустим сейчас 08:00, Hakon должен идти работать.

```text id="2k5b75"
NpcRoutineService:
  вычисляет active entry = WorkAtForge

NpcBehaviorController:
  если текущий state не WorkAtForge:
      Exit old state
      Enter WorkAtForgeState

WorkAtForgeState.Enter:
  найти forge activity point
  TravelToLocation(forge_point)

NpcTravelController:
  SetDestination(forge_point.position)
  дождаться прибытия

WorkAtForgeState:
  занять point
  повернуть NPC
  проиграть work animation
  держать состояние до конца временного окна
```

Если в 12:00 routine меняется:

```text id="lgryoe"
RoutineService:
  active entry = EatLunch

BehaviorController:
  Exit WorkAtForgeState
  освобождает forge point
  Enter EatLunchState
  TravelToLocation(tavern_table)
```

Если игрок заговорил:

```text id="fc1w5l"
DialogueState прерывает WorkAtForgeState
NPC входит в TalkState
после разговора:
  если нет других high-priority states:
      RoutineService снова активирует актуальную routine entry
      NPC возвращается к forge/eat/sleep
```

Это прямо соответствует Gothic-идее: routine state прерывается разговором, уроном, perception/state change, а когда внештатное состояние заканчивается, NPC возвращается к daily routine. ([ataulien.github.io][2])

---

# 10. Сопровождение и “веди игрока” — это тоже routine-like states

Тут важный момент: сопровождение — не отдельная “особая система”, а тип behavior state.

В Gothic-разборе даже “following the player” и “guiding the player to some location” указаны как необычные, но всё равно связанные с daily routine/script-state элементы. ([ataulien.github.io][2])

Я бы сделал так:

```text id="49g7xx"
FollowPlayerState
  NPC держится рядом с игроком
  если игрок далеко — догоняет
  если игрок слишком близко — останавливается
  если бой — combat interrupt
  если destination reached или quest command — завершение

GuidePlayerState
  NPC идёт по route/path
  если игрок отстал — ждёт
  если игрок подошёл — продолжает
  если attacked — interrupt
  если дошёл — world command / quest stage
```

Пример Yarn:

```yarn id="tyf2ni"
<<start_behavior "hakon" "guide_player" "old_mine_entrance_route">>
```

Но лучше типизированно:

```yarn id="5ui2d1"
<<guide_player "hakon" "route_to_old_mine">>
```

C#:

```text id="4ujwlr"
CharacterWorldCommandHandler
  -> find live instance
  -> NpcBehaviorController.PushState(GuidePlayerState)
  -> GuidePlayerState uses NpcTravelController.FollowRoute()
```

---

# 11. Patrol — не просто loop SetDestination

Патруль должен быть отдельным behavior state, потому что у него есть дополнительные правила:

```text id="29r6b0"
какой route
loop/pingpong/random
ждать ли на точках
куда смотреть на точках
замечать ли игрока
менять ли скорость
что делать при тревоге
возвращаться ли к маршруту после боя
```

Пример:

```csharp id="fdocxp"
public sealed class PatrolRouteDefinitionSO : ScriptableObject
{
    public string PatrolId;
    public List<PatrolNode> Nodes;
    public PatrolMode Mode;
    public bool ResumeFromNearestNodeAfterInterrupt;
}

[Serializable]
public sealed class PatrolNode
{
    public string LocationId;
    public float WaitSeconds;
    public string LookAtId;
    public string AnimationId;
    public PatrolAlertness Alertness;
}
```

Для стражника:

```text id="h46r9v"
08:00–20:00 GuardPatrol old_gate_patrol
20:00–22:00 SitCampfire guard_fire_01
22:00–08:00 Sleep barracks_bed_03
```

---

# 12. Как решить “передвижение по большой карте”

Тут нужно разделить **online** и **offline** симуляцию.

## Online: NPC рядом с игроком / в загруженной сцене

Тогда NPC существует как GameObject и физически идёт:

```text id="mad5io"
NavMeshAgent
TravelController
BehaviorState
Animations
Avoidance
```

## Offline: NPC далеко / сцена не загружена

Тогда не надо держать агента и считать NavMesh. Нужно обновлять только логическое состояние.

Например NPC должен в 08:00 быть у кузницы, а игрок пришёл туда в 09:30.

Ты не обязан симулировать весь путь NPC с кровати до кузницы. Можно сделать:

```text id="yqhi7f"
RoutineProjectionService:
  по текущему времени вычисляет expected location/activity
  CharacterWorldState.LocationId = forge_area
  when scene loads:
      CharacterWorldPresenter spawns NPC at forge activity point
```

Это особенно важно для большого мира. Полная симуляция всех NPC по NavMesh вне зоны игрока — дорого и почти всегда не нужно.

---

# 13. Но что если игрок может встретить NPC по дороге?

Вот тут появляется интересная архитектура.

У тебя есть три уровня точности:

## Уровень 1 — простая проекция

Если NPC не загружен:

```text id="vyqese"
08:00 = forge
12:00 = tavern
18:00 = campfire
```

Игрок не увидит переход между ними, если не находится рядом.

Это достаточно для MVP.

## Уровень 2 — travel windows

Ты явно задаёшь, что между 07:45 и 08:10 NPC “в пути”.

```text id="5wdd8f"
07:45–08:10 Travel home_to_forge_route
08:10–12:00 Work forge
```

Если игрок встречает NPC на маршруте, ты можешь заспавнить его на ближайшей точке route с учётом времени.

```text id="7j0jvo"
progress = (currentTime - travelStart) / travelDuration
spawn near route point at progress
```

Это уже даёт иллюзию живого мира.

## Уровень 3 — persistent travel state

Для важных NPC ты сохраняешь travel-команду:

```csharp id="nngx4e"
public sealed class CharacterTravelState
{
    public string RouteId;
    public int StartedAtMinute;
    public int ExpectedArrivalMinute;
    public string FromLocationId;
    public string ToLocationId;
    public float Progress01;
}
```

Если игрок сохраняет игру, пока бандит убегает, после загрузки ты можешь восстановить:

```text id="cpq182"
если сцена загружена рядом — заспавнить на route progress
если прошло достаточно времени — считать прибывшим
```

Я бы делал уровень 1 сейчас, уровень 2 для важных городских NPC, уровень 3 только для сюжетных перемещений типа побега, сопровождения, конвоя.

---

# 14. Очень важное решение: routine не должна постоянно менять `WorldCharacterState.LocationId`

Я бы разделил:

```text id="c0i9b1"
Home/Anchor LocationId
Current Logical LocationId
Current Activity
Current Travel Task
```

Если NPC идёт от дома к кузнице, не обязательно каждую секунду писать в `WorldCharacterState.LocationId`.

Лучше так:

```text id="40g57x"
LocationId = "hakon_home" или "hakon_forge" после прибытия
CurrentTravel = home_to_forge
CurrentActivity = Traveling
```

Для live instance позиция берётся из Transform/NavMeshAgent.

Для save/load:

```text id="1p4v8w"
если CurrentTravel есть:
  восстановить по route/progress
иначе:
  spawn at LocationId
```

Иначе `LocationId` превратится в странную штуку: то логическое место, то фактическая текущая позиция, то destination.

Я бы сделал так:

```csharp id="k5e8ja"
public sealed class WorldCharacterState
{
    public string CharacterId;
    public bool IsAlive;
    public bool IsPresent;

    public string LocationId; // последняя стабильная/логическая локация
    public string ActivityId; // sleep/work/patrol/travel/etc

    public CharacterTravelState? Travel;
}
```

---

# 15. Команды должны различать instant relocation и physical travel

Ты уже это правильно сформулировал ранее. Я бы закрепил API:

```yarn id="swtrn7"
<<move_character "hakon" "forge">>
```

Смысл:

```text id="q5udxc"
Мгновенно изменить логическую позицию.
Используется для offscreen, save load, chapter change, teleport-like story changes.
```

```yarn id="a82ap4"
<<send_character "hakon" "forge">>
```

Смысл:

```text id="ab4jke"
Дать physical travel command.
Если NPC заспавнен рядом — он реально идёт.
Если он offscreen — создаётся travel state или сразу projected arrival.
```

```yarn id="bjpy7t"
<<guide_player "hakon" "route_to_mine">>
```

Смысл:

```text id="5de9ic"
Включить behavior state GuidePlayerState.
NPC идёт по маршруту и учитывает игрока.
```

```yarn id="9wgqq7"
<<start_patrol "guard_01" "old_gate_patrol">>
```

Смысл:

```text id="jc6k98"
Прервать обычную routine и дать временную patrol behavior.
```

---

# 16. Как бы я реализовал минимальный MVP

## Data

```text id="rgxpf2"
WorldLocationAnchor
  LocationId
  Transform

NpcRoutineDefinitionSO
  entries by time

NpcRouteDefinitionSO
  nodes: LocationId + wait + movement mode

NpcActivityPoint
  ActivityType
  StandPoint
  LookAtPoint
  Occupancy
```

## Runtime

```text id="lwef0c"
NpcRoutineService
  determines current routine entry

NpcBehaviorController
  owns current behavior state
  supports PushState/PopState/SetRoutineState

NpcTravelController
  TravelToLocation
  FollowRoute
  FollowTarget
  Stop

NavMeshMotor
  wraps NavMeshAgent
```

## States

Начни с малого:

```text id="naqi45"
TravelToLocationState
PatrolState
WorkAtPointState
SitState
SleepState
TalkState
FollowPlayerState
GuidePlayerState
FleeState
```

Не делай сразу behavior tree. Тебе сейчас выгоднее явные state-классы.

---

# 17. Пример потока: обычная рутина

```text id="w6lti8"
08:00
RoutineService finds:
  WorkAtPoint forge_area

BehaviorController:
  exits SleepState
  enters WorkAtPointState

WorkAtPointState:
  asks ActivityPointRegistry for free "forge" point near forge_area
  reserves point
  TravelController.TravelToLocation(point.StandPoint)

TravelController:
  NavMeshAgent.SetDestination(point.StandPoint.position)

When arrived:
  Motor stops
  NPC rotates to point.LookAt
  AnimationController plays "forge_work"

12:00
Routine switches to EatState
WorkAtPointState.Exit:
  stops animation
  releases forge point
```

---

# 18. Пример потока: патруль

```text id="ah5g0n"
Routine entry:
  08:00–20:00 Patrol old_gate_patrol

PatrolState.Enter:
  load route old_gate_patrol
  choose start node:
    nearest node / first node / saved node

Loop:
  TravelToLocation(currentNode)
  wait node.WaitSeconds
  look at node.LookAt
  play optional animation
  advance next node

On interruption:
  save current node index if needed
  Stop travel

After dialogue/combat:
  resume from nearest or saved node
```

---

# 19. Пример потока: сопровождение игрока

```text id="d3u61w"
Yarn:
  <<guide_player "hakon" "route_to_old_mine">>

GuidePlayerState.Enter:
  route = route_to_old_mine
  currentNode = 0

Loop:
  if player too far behind:
      stop and wait
      play "wait" bark: "Hurry up"
  else:
      TravelToLocation(route[currentNode])
  if reached node:
      currentNode++
  if route complete:
      complete quest stage / set fact
      return to routine
```

Сопровождение — это не патруль, потому что здесь есть зависимость от игрока.

---

# 20. Пример потока: побег бандита

```text id="bpvoy8"
Yarn:
  <<send_character "bandit" "city_bandit_escape_01">>
```

Внутри:

```text id="uswtqn"
если bandit live:
  PushState(FleeToLocationState)
  TravelToLocation(city_bandit_escape_01, Run)
  когда дошёл:
      CharacterWorldState.LocationId = city_bandit_escape_01
      IsPresent = false или true, зависит от дизайна
      set_fact bandit_escaped

если bandit offscreen:
  CharacterWorldState.Travel = escape_travel_state
  или сразу LocationId = city_bandit_escape_01
```

---

# 21. Что делать с “заранее подготовленным маршрутом”

Моё правило:

```text id="5sdl4r"
Для смысла — RouteDefinition.
Для физического обхода препятствий — NavMesh.
Для красивого/контролируемого пути — AuthoredPath с промежуточными nodes.
Для кат-сцен — scripted/root-motion/spline.
```

То есть тебе не нужно заменять NavMesh. Тебе нужно **обернуть его в authored travel system**.

NavMesh отвечает за:

```text id="q3q4jl"
обойти ящик
обойти игрока
найти проход вокруг стены
корректно идти по земле
```

Route отвечает за:

```text id="g6y9kg"
идти через северные ворота, а не через южные
сначала проверить склад
потом пройти к рынку
потом вернуться к посту
```

Behavior отвечает за:

```text id="3xmo11"
почему он это делает
что делать на точках
можно ли его прервать
что делать после завершения
```

---

# 22. Очень практический совет для Unity

Не полагайся только на `remainingDistance`.

В Unity `remainingDistance` — это расстояние до destination по текущему path, но на практике важно проверять ещё `pathPending`, `hasPath`, `pathStatus`, скорость и `stoppingDistance`. В официальном API есть все эти свойства: `pathPending`, `pathStatus`, `remainingDistance`, `stoppingDistance`, `hasPath`, `velocity`. ([docs.unity3d.com][1])

Я бы сделал helper:

```csharp id="apm4vs"
public bool HasArrived()
{
    if (_agent.pathPending)
        return false;

    if (_agent.pathStatus == NavMeshPathStatus.PathInvalid)
        return false;

    if (_agent.remainingDistance > _agent.stoppingDistance)
        return false;

    if (_agent.hasPath && _agent.velocity.sqrMagnitude > 0.01f)
        return false;

    return true;
}
```

И не размазывал эту проверку по behavior states.

---

# 23. Моя главная рекомендация для твоего проекта

Не делай “Routine = список locationId”. Делай:

```text id="rj01p8"
Routine = расписание behavior states.
BehaviorState = смысловое действие.
TravelController = исполнитель перемещений.
Route = authored список смысловых узлов.
NavMesh = низкоуровневый pathfinder между узлами.
```

И тогда у тебя естественно появятся все Gothic-like сценарии:

```text id="lsvx0z"
NPC работает у кузницы
NPC идёт в таверну
NPC садится на лавку
NPC ищет свободное место у костра
NPC патрулирует ворота
NPC ведёт игрока в шахту
NPC ждёт, если игрок отстал
NPC убегает после диалога
NPC возвращается к routine после разговора
NPC offscreen логически оказывается там, где должен быть по времени
```

Самое важное архитектурное правило:

```text id="q5j3yd"
WorldCharacterState хранит логическое состояние.
RoutineService выбирает, что NPC должен делать.
BehaviorState исполняет смысловое действие.
TravelController решает, как идти.
NavMeshAgent только строит путь между ближайшими физическими точками.
```

Это даст тебе Gothic-like систему без попытки превратить NavMesh в “искусственный интеллект”.

[1]: https://docs.unity3d.com/6000.1/Documentation/ScriptReference/AI.NavMeshAgent.html "Unity - Scripting API: NavMeshAgent"
[2]: https://ataulien.github.io/Inside-Gothic/DailyRoutine/ "Daily Routine - Inside-Gothic"
[3]: https://mdk.gothicarchive.org/docs/skripte/gothic_skripte.htm "GothicMDK | Scripts"
