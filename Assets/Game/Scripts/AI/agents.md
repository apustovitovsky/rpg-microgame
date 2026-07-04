Да. Если пока оставляешь текущий `MovementController`, то правильная цепочка должна быть не “NavMeshAgent управляет MovementController”, а:

```text id="k37zn2"
NpcBrain / Task / UnityBehaviour
  -> INavigationCommand / NavMeshPlanner.GoTo(point)

NavMeshPlanner
  -> NavMeshAgent.SetDestination(point)
  -> считает desired world movement direction

NavMeshActorInput : IActorInput
  -> конвертирует desired world direction в MoveComposite
  -> при необходимости управляет sprint/walk/aim events

MovementController
  -> уже как сейчас читает IActorInput.MoveComposite
  -> двигает CharacterController

ActorLookController
  -> уже как сейчас читает IActorInput.LookDelta
  -> или получает explicit SetTarget / SetWorldDirection
```

То есть **команда “иди туда” должна приходить в navigation/planner слой**, а не в `MovementController`.

## Почему именно так под твой код

У тебя `MovementController` уже bind-ится к `IActorInput`, хранит `_input` и считает движение из `_input.MoveComposite`, `_look.ForwardFlatNormalized`, `_look.RightFlatNormalized`. Значит он ожидает не world position и не path, а **input vector**. ([GitHub][1])

`ActorLookController` тоже bind-ится к `IActorInput`, читает `_input.LookDelta`, но также уже имеет `SetTarget(Transform target)` / `ClearTarget()`. То есть look можно вести либо input-дельтой, либо explicit target-ом. ([GitHub][2])

`IActorInput` у тебя содержит именно `LookDelta`, `MoveComposite`, `MovementInputDuration`, `MovementInputDetected` и события aim/crouch/jump/lock/sprint/walk. Поэтому NPC/NavMesh-управление должно подделывать **input source**, а не ломать controller API. ([GitHub][3])

## Компоненты на NPC prefab

Я бы сделал так:

```text id="2ef3u3"
PF_Npc
  ActorView

  MovementController
  ActorLookController

  CharacterController
  NavMeshAgent

  NavMeshPlanner
  NavMeshActorInput
  NpcBrain / TaskRunner / DebugGoToPointBehaviour

  NpcActorCompositionRoot
```

Где роли такие:

```text id="3foc6z"
MovementController
  ничего не знает про NavMesh

ActorLookController
  ничего не знает про NavMesh

NavMeshAgent
  только строит путь / steering
  updatePosition = false
  updateRotation = false

NavMeshPlanner
  владеет destination/path state
  вызывает agent.SetDestination
  читает agent.desiredVelocity / steeringTarget
  публикует DesiredWorldDirection / HasArrived

NavMeshActorInput
  реализует IActorInput
  читает NavMeshPlanner.DesiredWorldDirection
  конвертирует world direction в MoveComposite

NpcBrain / UnityBehaviour
  дает команду planner.GoTo(point)

CompositionRoot
  связывает movement/look с input
```

## Зависимости

Стрелка означает “знает про / имеет ссылку на”.

```text id="80pdnv"
NpcBrain
  -> INpcNavigation

INpcNavigation implemented by NavMeshPlanner
  -> NavMeshAgent

NavMeshActorInput
  -> INpcNavigation
  -> ActorLookController

MovementController
  -> IActorInput
  -> ActorLookController

ActorLookController
  -> IActorInput
```

Самое важное:

```text id="j969f4"
NpcBrain не знает MovementController
NpcBrain не знает ActorLookController
MovementController не знает NavMeshAgent
ActorLookController не знает NavMeshAgent
NavMeshAgent не двигает transform напрямую
```

## Интерфейс для команды “иди к точке”

```csharp id="lh5pex"
using UnityEngine;

namespace Game.Actor.Navigation
{
    public interface INpcNavigation
    {
        bool HasDestination { get; }
        bool IsMoving { get; }
        bool HasArrived { get; }
        Vector3 DesiredWorldDirection { get; }

        void GoTo(Vector3 position);
        void Stop();
    }
}
```

Это именно то, что должен дергать `NpcBrain`, task или любой Unity behaviour.

## `NavMeshPlanner`

Он владеет `NavMeshAgent`, но не двигает персонажа сам.

```csharp id="zmmyo6"
using UnityEngine;
using UnityEngine.AI;

namespace Game.Actor.Navigation
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class NavMeshPlanner : MonoBehaviour, INpcNavigation
    {
        [SerializeField] private NavMeshAgent _agent;
        [SerializeField] private float _arrivalTolerance = 0.15f;
        [SerializeField] private float _repathDistance = 0.25f;

        private Vector3 _lastDestination;
        private bool _hasDestination;
        private bool _hasArrived;

        public bool HasDestination => _hasDestination;
        public bool HasArrived => _hasArrived;
        public bool IsMoving => _hasDestination && !_hasArrived;

        public Vector3 DesiredWorldDirection { get; private set; }

        private void Awake()
        {
            if (_agent == null)
                _agent = GetComponent<NavMeshAgent>();

            _agent.updatePosition = false;
            _agent.updateRotation = false;
        }

        private void Update()
        {
            DesiredWorldDirection = Vector3.zero;

            if (!_hasDestination || !_agent.enabled || !_agent.isOnNavMesh)
                return;

            // Внутренняя simulated position агента должна следовать за реальным root,
            // потому что root двигает MovementController через CharacterController.
            _agent.nextPosition = transform.position;

            if (_agent.pathPending)
                return;

            if (CheckArrived())
            {
                Stop();
                _hasArrived = true;
                return;
            }

            DesiredWorldDirection = CalculateDesiredWorldDirection();
        }

        private void LateUpdate()
        {
            if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
                _agent.nextPosition = transform.position;
        }

        public void GoTo(Vector3 position)
        {
            if (_agent == null || !_agent.enabled || !_agent.isOnNavMesh)
                return;

            bool shouldRepath =
                !_hasDestination ||
                Vector3.Distance(_lastDestination, position) >= _repathDistance;

            _hasDestination = true;
            _hasArrived = false;
            _lastDestination = position;

            if (!shouldRepath)
                return;

            _agent.isStopped = false;
            _agent.SetDestination(position);
        }

        public void Stop()
        {
            _hasDestination = false;
            DesiredWorldDirection = Vector3.zero;

            if (_agent == null || !_agent.enabled || !_agent.isOnNavMesh)
                return;

            _agent.isStopped = true;
            _agent.ResetPath();
            _agent.nextPosition = transform.position;
        }

        private bool CheckArrived()
        {
            if (_agent.pathPending)
                return false;

            if (!_agent.hasPath)
                return true;

            float stoppingDistance = Mathf.Max(
                _agent.stoppingDistance,
                _arrivalTolerance);

            return _agent.remainingDistance <= stoppingDistance;
        }

        private Vector3 CalculateDesiredWorldDirection()
        {
            Vector3 desiredVelocity = _agent.desiredVelocity;
            desiredVelocity.y = 0f;

            if (desiredVelocity.sqrMagnitude > 0.0001f)
                return desiredVelocity.normalized;

            Vector3 toSteeringTarget = _agent.steeringTarget - transform.position;
            toSteeringTarget.y = 0f;

            return toSteeringTarget.sqrMagnitude > 0.0001f
                ? toSteeringTarget.normalized
                : Vector3.zero;
        }
    }
}
```

`NavMeshAgent` должен быть в manual transform mode: `updatePosition = false`, потому что реальное перемещение делает твой `MovementController` через `CharacterController.Move`. Unity описывает `NavMeshAgent.updatePosition = false` как режим, где transform контролируется скриптом, а simulated position агента можно использовать отдельно. `nextPosition` при этом является simulated position агента, которую можно синхронизировать с реальным transform. ([GitHub][4])

## `NavMeshActorInput`

Этот класс реализует твой `IActorInput`. Он берет world direction из planner-а и превращает ее в `MoveComposite`, потому что твой `MovementController` ожидает input относительно look basis:

```csharp id="6swsg3"
_moveDirection =
    _look.ForwardFlatNormalized * _input.MoveComposite.y +
    _look.RightFlatNormalized * _input.MoveComposite.x;
```

То есть для NPC нужно сделать обратное преобразование:

```text id="h1da4d"
world direction
  -> dot with look right   = input.x
  -> dot with look forward = input.y
```

Код:

```csharp id="rm87hx"
using System;
using Game.Input;
using UnityEngine;

namespace Game.Actor.Navigation
{
    [DisallowMultipleComponent]
    public sealed class NavMeshActorInput : MonoBehaviour, IActorInput
    {
        [SerializeField] private NavMeshPlanner _planner;
        [SerializeField] private ActorLookController _look;

        [Header("Movement")]
        [SerializeField] private float _deadZone = 0.05f;
        [SerializeField] private bool _faceMoveDirection = true;
        [SerializeField] private bool _runByDefault = true;

        private bool _isSprinting;
        private bool _isWalking;

        public Vector2 LookDelta { get; private set; }
        public Vector2 MoveComposite { get; private set; }

        public float MovementInputDuration { get; set; }

        public bool MovementInputDetected =>
            MoveComposite.sqrMagnitude > _deadZone * _deadZone;

        public event Action OnAimActivated;
        public event Action OnAimDeactivated;
        public event Action OnCrouchActivated;
        public event Action OnCrouchDeactivated;
        public event Action OnJumpPerformed;
        public event Action OnLockOnToggled;
        public event Action OnSprintActivated;
        public event Action OnSprintDeactivated;
        public event Action OnWalkToggled;

        private void Update()
        {
            LookDelta = Vector2.zero;
            MoveComposite = Vector2.zero;

            if (_planner == null || !_planner.IsMoving)
                return;

            Vector3 desired = _planner.DesiredWorldDirection;

            if (desired.sqrMagnitude <= 0.0001f)
                return;

            if (_faceMoveDirection && _look != null)
                FaceWorldDirection(desired);

            MoveComposite = ToLookRelativeMove(desired);

            if (_runByDefault && _isWalking)
                ToggleWalk();
        }

        public void SetSprint(bool enabled)
        {
            if (_isSprinting == enabled)
                return;

            _isSprinting = enabled;

            if (_isSprinting)
                OnSprintActivated?.Invoke();
            else
                OnSprintDeactivated?.Invoke();
        }

        public void ToggleWalk()
        {
            _isWalking = !_isWalking;
            OnWalkToggled?.Invoke();
        }

        public void StopMovementInput()
        {
            MoveComposite = Vector2.zero;
            MovementInputDuration = 0f;

            if (_isSprinting)
                SetSprint(false);
        }

        private Vector2 ToLookRelativeMove(Vector3 worldDirection)
        {
            Vector3 forward = _look != null
                ? _look.ForwardFlatNormalized
                : transform.forward;

            Vector3 right = _look != null
                ? _look.RightFlatNormalized
                : transform.right;

            forward.y = 0f;
            right.y = 0f;

            forward = forward.sqrMagnitude > 0.0001f
                ? forward.normalized
                : transform.forward;

            right = right.sqrMagnitude > 0.0001f
                ? right.normalized
                : transform.right;

            Vector2 input = new(
                Vector3.Dot(worldDirection, right),
                Vector3.Dot(worldDirection, forward));

            return Vector2.ClampMagnitude(input, 1f);
        }

        private void FaceWorldDirection(Vector3 worldDirection)
        {
            // Нужен маленький public API в ActorLookController.
            // См. ниже SetWorldYaw/SetWorldDirection.
            _look.SetWorldDirection(worldDirection);
        }
    }
}
```

## Нужен маленький API в `ActorLookController`

Сейчас `ActorLookController` умеет смотреть через `LookDelta` или на target через `SetTarget`. Для NPC movement-facing лучше добавить метод:

```csharp id="yy5m61"
public void SetWorldDirection(Vector3 worldDirection)
{
    worldDirection.y = 0f;

    if (worldDirection.sqrMagnitude <= 0.0001f)
        return;

    ClearTarget();

    Quaternion rotation = Quaternion.LookRotation(worldDirection.normalized);
    _yaw = rotation.eulerAngles.y;
}
```

Тогда `NavMeshActorInput` может делать:

```text id="lfz2rw"
planner says: идти на северо-восток
look turns to north-east
MoveComposite becomes roughly (0, 1)
MovementController moves forward
```

Без этого NPC может пытаться идти в сторону через strafe input, потому что movement у тебя look-relative.

## Binder / CompositionRoot

Чтобы не было хаоса в `Awake`, сделай один компонент, который связывает input с controllers:

```csharp id="7kksso"
using UnityEngine;

namespace Game.Actor.Navigation
{
    [DisallowMultipleComponent]
    public sealed class NpcActorCompositionRoot : MonoBehaviour
    {
        [SerializeField] private NavMeshActorInput _input;
        [SerializeField] private MovementController _movement;
        [SerializeField] private ActorLookController _look;

        private void Awake()
        {
            _movement.Bind(_input);
            _look.Bind(_input);
        }

        private void OnDestroy()
        {
            _movement.Unbind();
            _look.Unbind();
        }
    }
}
```

Это важно: `MovementController` и `LookController` не должны сами искать input. Кто собрал prefab — тот и bind-ит.

## NpcBrain / Task / любой UnityBehaviour

Теперь команда “иди к точке” выглядит просто.

```csharp id="6ezj5c"
using Game.Actor.Navigation;
using UnityEngine;

namespace Game.Actor.AI
{
    public sealed class NpcBrain : MonoBehaviour
    {
        [SerializeField] private NavMeshPlanner _navigation;

        public void GoToPoint(Vector3 point)
        {
            _navigation.GoTo(point);
        }

        public void Stop()
        {
            _navigation.Stop();
        }

        private void Update()
        {
            if (_navigation.HasArrived)
            {
                // перейти к следующей задаче
            }
        }
    }
}
```

Или debug behaviour:

```csharp id="kt79ee"
using Game.Actor.Navigation;
using UnityEngine;

public sealed class DebugNpcGoToPoint : MonoBehaviour
{
    [SerializeField] private NavMeshPlanner _navigation;
    [SerializeField] private Transform _point;

    private void Update()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.G) && _point != null)
            _navigation.GoTo(_point.position);
    }
}
```

## Где должен жить `NavMeshPlanner`

Я бы не называл его `NpcBrain`.

`NpcBrain` отвечает за **что делать**:

```text id="fimghm"
патрулировать
идти к waypoint
атаковать
говорить
искать предмет
```

`NavMeshPlanner` отвечает за **как дойти**:

```text id="nlm52h"
SetDestination
remainingDistance
desiredVelocity
arrival
path pending
stop
```

То есть:

```text id="fyh0la"
NpcBrain
  -> GoTo(task.targetPosition)

NavMeshPlanner
  -> path/agent state

NavMeshActorInput
  -> движение как input

MovementController
  -> физическое движение
```

## Кто должен выставлять `agent.speed`

Для начала можно сделать просто:

```csharp id="1sj5qg"
_agent.speed = _runSpeed;
```

Но архитектурно лучше, чтобы `NavMeshPlanner` не знал настройки movement. Для MVP допустим один из двух вариантов:

### Простой вариант

В `NavMeshPlanner` serialized speed:

```csharp id="xw8iee"
[SerializeField] private float _agentSpeed = 2.5f;

private void Awake()
{
    _agent.speed = _agentSpeed;
}
```

И руками держишь его равным `_runSpeed`.

### Лучше

Сделать маленький read-only API в movement:

```csharp id="r0g6uv"
public float CurrentMaxSpeed => _currentMaxSpeed;
public float RunSpeed => _runSpeed;
```

И отдельный sync-компонент:

```csharp id="pfau3f"
public sealed class NavMeshMovementSpeedSync : MonoBehaviour
{
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private MovementController _movement;

    private void LateUpdate()
    {
        _agent.speed = _movement.CurrentMaxSpeed;
    }
}
```

Но для MVP я бы не усложнял: задай одинаковые значения в inspector.

## Итоговая цепочка вызовов на кадре

```text id="3ejxli"
NpcBrain:
  если задача началась -> NavMeshPlanner.GoTo(destination)

NavMeshPlanner.Update:
  agent.nextPosition = actor.position
  agent.SetDestination уже задан
  DesiredWorldDirection = agent.desiredVelocity.normalized

NavMeshActorInput.Update:
  desired = planner.DesiredWorldDirection
  look.SetWorldDirection(desired)
  MoveComposite = ToLookRelativeMove(desired)

MovementController.Update:
  CalculateInput()
  _moveDirection = look.forward * MoveComposite.y + look.right * MoveComposite.x
  CharacterController.Move(...)

ActorLookController.Update:
  обновляет Pivot yaw/pitch/position

NavMeshPlanner.LateUpdate:
  agent.nextPosition = actor.position
```

## Важный нюанс порядка Update

Идеально, чтобы порядок был:

```text id="y751xm"
1. NavMeshPlanner.Update
2. NavMeshActorInput.Update
3. ActorLookController.Update
4. MovementController.Update
5. NavMeshPlanner.LateUpdate
```

Но в Unity порядок `Update` между компонентами без Script Execution Order не гарантирует удобную последовательность. Для MVP чаще всего будет работать и так, но если будут задержки/дерганье, зафиксируй execution order:

```text id="c00rhj"
NavMeshPlanner        -200
NavMeshActorInput     -100
ActorLookController      0
MovementController     100
```

Или лучше в будущем перейти на явный `Tick()` из composition root:

```text id="3xsx6y"
NpcActorController.Update:
  planner.Tick()
  input.Tick()
  look.Tick()
  movement.Tick()
```

Но это уже следующий рефактор.

## Короткий вердикт

Для текущего контроллера лучшая зависимость такая:

```text id="wce2aa"
UnityBehaviour
  -> NavMeshPlanner.GoTo(point)

NavMeshPlanner
  -> NavMeshAgent

NavMeshActorInput : IActorInput
  -> NavMeshPlanner
  -> ActorLookController

MovementController
  -> IActorInput
  -> ActorLookController

ActorLookController
  -> IActorInput
```

`NavMeshAgent` не должен напрямую двигать actor root, `MovementController` не должен знать про `NavMeshAgent`, а `NpcBrain` не должен заниматься конвертацией path direction в input. Это и будет нормальная MVP-цепочка под твою текущую input-driven архитектуру.

