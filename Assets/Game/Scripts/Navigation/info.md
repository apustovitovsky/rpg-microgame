Да. В твоей текущей архитектуре `NavMeshAgent` **не должен управлять `MovementController` напрямую методами типа `MoveTo()`**, потому что `MovementController` уже построен вокруг `IActorInput`.

Значит правильный MVP-ход: **сделать AI/NavMesh input provider, который реализует `IActorInput`, и bind-ить его в `MovementController` и `ActorLookController` так же, как player input**.

То есть агент управляет не контроллером, а **input source**.

## Что у тебя сейчас важно

`MovementController` уже умеет принимать `IActorInput` через `Bind(IActorInput input)` и подписывается на события walk/sprint/crouch/aim. Он хранит `_input`, а движение считает из `_input.MoveComposite` и `_look.ForwardFlatNormalized / _look.RightFlatNormalized`. ([GitHub][1])

`ActorLookController` тоже принимает `IActorInput` через `Bind(IActorInput input)` и каждый `Update()` читает `_input.LookDelta`; кроме этого, у него уже есть `SetTarget(Transform target)` / `ClearTarget()`. ([GitHub][2])

`IActorInput` у тебя минимальный: `LookDelta`, `MoveComposite`, `MovementInputDuration`, `MovementInputDetected` и events для aim/crouch/jump/lock/sprint/walk. ([GitHub][3])

Значит цепочка для NPC должна быть такая:

```text
NavMeshAgent
  -> NavMeshActorInput : IActorInput
  -> MovementController.Bind(input)
  -> LookController.Bind(input)
```

А не такая:

```text
NavMeshAgent
  -> MovementController.private fields
  -> LookController.private fields
```

## Главная проблема: твой movement input camera/look-relative

У тебя движение считается так:

```csharp
_moveDirection =
    (_look.ForwardFlatNormalized * _input.MoveComposite.y) +
    (_look.RightFlatNormalized * _input.MoveComposite.x);
```

То есть `MoveComposite` — это не world direction. Это “локальный input относительно look pivot”.

Для игрока это нормально:

```text
W = двигаться вперед относительно камеры/look
D = двигаться вправо относительно камеры/look
```

Для NavMeshAgent это значит: если агент хочет идти в world direction, например `worldDesired = (1, 0, 0)`, тебе нужно перевести этот world direction в `MoveComposite` относительно текущего look basis:

```csharp
x = Dot(worldDesired, look.RightFlatNormalized)
y = Dot(worldDesired, look.ForwardFlatNormalized)
```

Именно это должен делать AI input provider.

## Минимальный `NavMeshActorInput`

```csharp
using System;
using Game.Input;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Actor.Navigation
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class NavMeshActorInput : MonoBehaviour, IActorInput
    {
        [Header("References")]
        [SerializeField] private NavMeshAgent _agent;
        [SerializeField] private ActorLookController _look;

        [Header("Input")]
        [SerializeField] private float _moveInputDeadZone = 0.05f;
        [SerializeField] private float _destinationRepathDistance = 0.25f;

        private Vector3 _destination;
        private bool _hasDestination;
        private bool _aimHeld;
        private bool _sprintHeld;
        private bool _walkEnabled;

        public Vector2 LookDelta { get; private set; }

        public Vector2 MoveComposite { get; private set; }

        public float MovementInputDuration { get; set; }

        public bool MovementInputDetected =>
            MoveComposite.sqrMagnitude > _moveInputDeadZone * _moveInputDeadZone;

        public event Action OnAimActivated;
        public event Action OnAimDeactivated;
        public event Action OnCrouchActivated;
        public event Action OnCrouchDeactivated;
        public event Action OnJumpPerformed;
        public event Action OnLockOnToggled;
        public event Action OnSprintActivated;
        public event Action OnSprintDeactivated;
        public event Action OnWalkToggled;

        private void Awake()
        {
            if (_agent == null)
                _agent = GetComponent<NavMeshAgent>();

            _agent.updatePosition = false;
            _agent.updateRotation = false;
        }

        private void Update()
        {
            LookDelta = Vector2.zero;
            MoveComposite = Vector2.zero;

            if (!_hasDestination || _agent == null || !_agent.enabled || !_agent.isOnNavMesh)
                return;

            _agent.nextPosition = transform.position;

            if (_agent.pathPending)
                return;

            if (HasArrived())
            {
                Stop();
                return;
            }

            Vector3 worldMoveDirection = GetDesiredWorldMoveDirection();

            if (worldMoveDirection.sqrMagnitude <= 0.0001f)
                return;

            MoveComposite = ToLookRelativeInput(worldMoveDirection);
        }

        private void LateUpdate()
        {
            if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
                _agent.nextPosition = transform.position;
        }

        public void SetDestination(Vector3 destination)
        {
            if (_agent == null || !_agent.enabled || !_agent.isOnNavMesh)
                return;

            bool shouldSet =
                !_hasDestination ||
                Vector3.Distance(_destination, destination) >= _destinationRepathDistance;

            _destination = destination;
            _hasDestination = true;

            if (shouldSet)
            {
                _agent.isStopped = false;
                _agent.SetDestination(destination);
            }
        }

        public void Stop()
        {
            _hasDestination = false;
            MoveComposite = Vector2.zero;
            MovementInputDuration = 0f;

            if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
            {
                _agent.isStopped = true;
                _agent.ResetPath();
                _agent.nextPosition = transform.position;
            }

            SetSprint(false);
            SetAim(false);
        }

        public void SetAim(bool enabled)
        {
            if (_aimHeld == enabled)
                return;

            _aimHeld = enabled;

            if (_aimHeld)
                OnAimActivated?.Invoke();
            else
                OnAimDeactivated?.Invoke();
        }

        public void SetSprint(bool enabled)
        {
            if (_sprintHeld == enabled)
                return;

            _sprintHeld = enabled;

            if (_sprintHeld)
                OnSprintActivated?.Invoke();
            else
                OnSprintDeactivated?.Invoke();
        }

        public void ToggleWalk()
        {
            _walkEnabled = !_walkEnabled;
            OnWalkToggled?.Invoke();
        }

        private bool HasArrived()
        {
            if (_agent.pathPending)
                return false;

            float stoppingDistance = Mathf.Max(_agent.stoppingDistance, 0.05f);

            return !_agent.hasPath ||
                   _agent.remainingDistance <= stoppingDistance;
        }

        private Vector3 GetDesiredWorldMoveDirection()
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

        private Vector2 ToLookRelativeInput(Vector3 worldDirection)
        {
            Vector3 lookForward = _look != null
                ? _look.ForwardFlatNormalized
                : transform.forward;

            Vector3 lookRight = _look != null
                ? _look.RightFlatNormalized
                : transform.right;

            lookForward.y = 0f;
            lookRight.y = 0f;

            lookForward = lookForward.sqrMagnitude > 0.0001f
                ? lookForward.normalized
                : transform.forward;

            lookRight = lookRight.sqrMagnitude > 0.0001f
                ? lookRight.normalized
                : transform.right;

            Vector2 input = new(
                Vector3.Dot(worldDirection, lookRight),
                Vector3.Dot(worldDirection, lookForward));

            return Vector2.ClampMagnitude(input, 1f);
        }
    }
}
```

## Как bind-ить

Для NPC нужен binder/composition root:

```csharp
using UnityEngine;

namespace Game.Actor.Navigation
{
    [DisallowMultipleComponent]
    public sealed class NpcActorInputBinder : MonoBehaviour
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

На prefab NPC:

```text
PF_Npc
  MovementController
  ActorLookController
  NavMeshAgent
  NavMeshActorInput
  NpcActorInputBinder
```

На player prefab:

```text
PF_Player
  MovementController
  ActorLookController
  PlayerActorInput
  PlayerActorInputBinder
```

То есть `MovementController` вообще не должен знать, player это или NPC.

## Но с текущим `LookController` есть важный нюанс

`ActorLookController` сейчас читает `LookDelta`. Если NPC input всегда возвращает `LookDelta = Vector2.zero`, то look pivot сам не повернется. А `MovementController` в strafing-режиме ориентируется на `_look.ForwardFlatNormalized`, и если `_alwaysStrafe = true`, root будет стремиться смотреть в look forward. У тебя `_alwaysStrafe` по умолчанию включен в настройках movement. ([GitHub][1])

Для NPC лучше один из двух вариантов.

## Вариант A: NPC идет как обычный non-strafe character

Для NPC отключаешь `_alwaysStrafe`.

Тогда `MovementController.FaceMoveDirection()` в не-strafe ветке повернет root по горизонтальной velocity:

```csharp
Vector3 faceDirection = new(_velocity.x, 0f, _velocity.z);
transform.rotation = Quaternion.Slerp(
    transform.rotation,
    Quaternion.LookRotation(faceDirection),
    _rotationSmoothing * Time.deltaTime);
```

Это уже есть в твоем коде. ([GitHub][1])

В этом варианте агенту достаточно кормить `MoveComposite`, а look можно оставить почти неподвижным или использовать только для target lock/dialogue.

Минус: так как `MoveComposite` считается относительно look, а look может быть не туда повернут, придется либо синхронизировать look forward с actor forward, либо уйти от look-relative input для AI. Поэтому это не самый надежный вариант на долгую перспективу.

## Вариант B: добавить в `ActorLookController` world-facing API

Это лучше.

Добавь в `ActorLookController` метод не через `LookDelta`, а через world direction:

```csharp
public void SetWorldDirection(Vector3 worldDirection)
{
    worldDirection.y = 0f;

    if (worldDirection.sqrMagnitude <= 0.0001f)
        return;

    ClearTarget();

    Quaternion rotation = Quaternion.LookRotation(worldDirection.normalized);
    Vector3 euler = rotation.eulerAngles;

    _yaw = euler.y;
}
```

И в `NavMeshActorInput`/navigation driver каждый кадр:

```csharp
Vector3 worldMoveDirection = GetDesiredWorldMoveDirection();
_look.SetWorldDirection(worldMoveDirection);
MoveComposite = ToLookRelativeInput(worldMoveDirection);
```

Тогда `MoveComposite` почти всегда будет `(0, 1)`, потому что look forward направлен туда же, куда хочет идти агент.

Это хорошо ложится на твою текущую математику:

```text
agent desired world direction
  -> look yaw turns to desired direction
  -> MoveComposite = forward
  -> MovementController moves along look forward
```

То есть NPC “нажимает W” в сторону маршрута.

## Еще чище: сделать отдельный input adapter, а NavMesh не должен быть `IActorInput`

Я бы даже разделил на два класса:

```text
NavMeshNavigator
  знает NavMeshAgent, destination, arrived, desired world direction

NpcActorInput
  реализует IActorInput
  получает desired world direction от navigator
  конвертирует его в MoveComposite
```

Так лучше по ответственности.

```csharp
public interface IActorNavigation
{
    bool HasDestination { get; }
    bool HasArrived { get; }
    Vector3 DesiredWorldDirection { get; }

    void SetDestination(Vector3 destination);
    void Stop();
}
```

Но для MVP можно оставить `NavMeshActorInput` как один класс.

## Что не надо делать

Не надо добавлять в `MovementController` прямые зависимости:

```csharp
[SerializeField] private NavMeshAgent _agent;
```

Хотя у тебя в `MovementController` уже есть `using UnityEngine.AI;`, сам controller не должен знать `NavMeshAgent`. Это будет плохой слой: player movement внезапно начнет зависеть от navigation. ([GitHub][1])

Не надо делать агент владельцем transform:

```csharp
agent.updatePosition = true;
agent.updateRotation = true;
```

Если ты оставляешь `CharacterController.Move()` внутри `MovementController`, потому что тогда опять будет два владельца позиции: `CharacterController` и `NavMeshAgent`. `NavMeshAgent.updatePosition = false` как раз нужен для режима, когда transform контролируется внешней системой, а agent position синхронизируется вручную через `nextPosition`. ([GitHub][4])

## Минимальная правильная цепочка для твоего проекта

```text
NpcBrain / Task
  -> NavMeshActorInput.SetDestination(position)

NavMeshActorInput : IActorInput
  -> reads NavMeshAgent.desiredVelocity / steeringTarget
  -> converts world direction to MoveComposite
  -> optionally tells LookController to face movement direction

MovementController
  -> reads IActorInput.MoveComposite
  -> applies speed/gait/gravity/CharacterController.Move

ActorLookController
  -> reads IActorInput.LookDelta
  -> or receives SetTarget / SetWorldDirection
```

## Что я бы конкретно изменил сейчас

1. Убрал бы `using UnityEngine.AI;` из `MovementController`, если он реально не используется.
2. Создал бы `NavMeshActorInput : MonoBehaviour, IActorInput`.
3. На NPC bind-ил бы `MovementController` и `ActorLookController` к `NavMeshActorInput`.
4. В `NavMeshAgent` поставил бы:

```csharp
_agent.updatePosition = false;
_agent.updateRotation = false;
```

5. Добавил бы в `ActorLookController` метод `SetWorldDirection(Vector3 direction)` или `SetYaw(float yaw)`.
6. В `NavMeshActorInput.Update()` делал бы:

```csharp
_agent.nextPosition = transform.position;

Vector3 desired = GetDesiredWorldMoveDirection();

_look.SetWorldDirection(desired);

MoveComposite = ToLookRelativeInput(desired);
```

Это наименее инвазивный вариант под твою текущую архитектуру: `MovementController` остается input-driven, `LookController` остается look-driven, а NavMesh становится просто еще одним источником input — не вторым movement controller.

[1]: https://github.com/apustovitovsky/rpg-microgame/blob/main/Assets/Game/Scripts/Actor/Components/Movement/MovementController.cs "rpg-microgame/Assets/Game/Scripts/Actor/Components/Movement/MovementController.cs at main · apustovitovsky/rpg-microgame · GitHub"
[2]: https://github.com/apustovitovsky/rpg-microgame/blob/main/Assets/Game/Scripts/Actor/Components/Look/LookController.cs "rpg-microgame/Assets/Game/Scripts/Actor/Components/Look/LookController.cs at main · apustovitovsky/rpg-microgame · GitHub"
[3]: https://github.com/apustovitovsky/rpg-microgame/blob/main/Assets/Game/Scripts/Input/Runtime/IActorInput.cs "rpg-microgame/Assets/Game/Scripts/Input/Runtime/IActorInput.cs at main · apustovitovsky/rpg-microgame · GitHub"
[4]: https://github.com/apustovitovsky/rpg-microgame/raw/refs/heads/main/Assets/Game/Scripts/Actor/Components/Movement/MovementController.cs "raw.githubusercontent.com"
