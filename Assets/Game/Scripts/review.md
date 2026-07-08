Я бы **не делал `ITargetable` registry ради distance check** и **не дублировал именно `TargetPoint` в `IInteractable`**.

Лучший вариант: ввести **interaction-specific spatial данные**, потому что `TargetPoint` и “точка взаимодействия” — не одно и то же.

Сейчас у тебя:

```csharp
ITargetable
  WorldId
  Root
  TargetPoint
  IsTargetable
```

а `IInteractable` хранит только `MaxRange`, `CanInteract`, `InteractAsync`. `IWorldSpatial` — это просто `Vector3 Position`. ([GitHub][1])

## Почему не `ITargetable registry`

`ITargetable` — это endpoint для **выбора/наведения/lock-on**.

```text
TargetPoint = куда смотреть / куда вешать UI / куда целиться
```

`IInteractable` — endpoint для **use/interact**.

```text
InteractionPoint = откуда считать дистанцию взаимодействия
```

У pickup они могут совпадать. У NPC/сундука/двери — не обязательно.

Например:

```text
NPC TargetPoint = голова/грудь
NPC InteractionPoint = root / точка перед NPC

Chest TargetPoint = центр сундука
Chest InteractionPoint = точка у крышки/перед сундуком

Door TargetPoint = центр двери
Door InteractionPoint = ручка или trigger-zone
```

Поэтому `TargetPoint` не надо превращать в универсальную точку взаимодействия.

## Что лучше сделать

Я бы добавил два interaction endpoint-а:

```csharp
public interface IInteractor
{
    WorldId WorldId { get; }

    Vector3 InteractionOrigin { get; }
}
```

```csharp
public interface IInteractable
{
    WorldId WorldId { get; }

    Vector3 InteractionPosition { get; }

    float MaxRange { get; }

    bool CanInteract(InteractionContext context);

    UniTask InteractAsync(
        InteractionContext context,
        CancellationToken token);
}
```

Тогда `InteractionService` зависит от:

```csharp
private readonly IWorldRegistry<IInteractor> _interactors;
private readonly IWorldRegistry<IInteractable> _interactables;
```

И проверка становится честной:

```csharp
if (!_interactors.TryGet(interactorWorldId, out var interactor))
    return false;

if (!_interactables.TryGet(targetWorldId, out var interactable))
    return false;

if (Vector3.Distance(
        interactor.InteractionOrigin,
        interactable.InteractionPosition) > interactable.MaxRange)
{
    return false;
}
```

## Почему это лучше, чем `IWorldSpatial`

`IWorldSpatial` слишком общий:

```text
IWorldSpatial.Position
```

Непонятно, что это за позиция:

```text
root?
center?
feet?
head?
interaction point?
target point?
bounds center?
```

Для distance check в interaction тебе нужна не “позиция объекта вообще”, а именно:

```text
позиция того, кто взаимодействует
позиция точки взаимодействия цели
```

Поэтому лучше не универсальный `IWorldSpatial`, а более точные интерфейсы:

```text
IInteractor.InteractionOrigin
IInteractable.InteractionPosition
```

## Где хранить `Transform`

В конкретных MonoBehaviour можно хранить serialized transform:

```csharp
[SerializeField] private Transform _interactionPoint;
```

но наружу лучше отдавать `Vector3`:

```csharp
public Vector3 InteractionPosition =>
    _interactionPoint != null
        ? _interactionPoint.position
        : transform.position;
```

То есть не так:

```csharp
IInteractable.TargetPoint
```

а так:

```csharp
IInteractable.InteractionPosition
```

## Итог

Я бы сделал так:

```text
Убрать из InteractionService:
  IWorldRegistry<IWorldSpatial>

Не добавлять:
  IWorldRegistry<ITargetable> ради дистанции

Добавить:
  IWorldRegistry<IInteractor>
  IWorldRegistry<IInteractable>

В IInteractable добавить:
  Vector3 InteractionPosition

В actor/player добавить:
  IInteractor с InteractionOrigin
```

Коротко: **`TargetPoint` — для targeting/UI/look. `InteractionPosition` — для interaction range. Не смешивай их.**

[1]: https://raw.githubusercontent.com/apustovitovsky/rpg-microgame/main/Assets/Game/Scripts/Interaction/Runtime/IInteractable.cs "raw.githubusercontent.com"
