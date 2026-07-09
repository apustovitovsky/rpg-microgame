Да, **`ITargetProvider` почти точно не должен быть отдельным глобальным реестром**.

Он не описывает объект мира как цель. Он описывает **состояние выбора цели у конкретного владельца**:

```csharp
public interface ITargetProvider
{
    ITargetable CurrentTarget { get; }
    event Action<ITargetable> CurrentTargetChanged;
}
```

То есть это не:

```text
WorldId -> targetable object
```

а:

```text
actor -> кого actor сейчас выбрал
```

Поэтому логичнее держать это **внутри actor runtime entity**, если targeting — способность актора.

## Лучше так

```csharp
public interface IWorldActor
{
    WorldId WorldId { get; }
    ActorDefinition Definition { get; }

    IActorView View { get; }

    IActorInputBinder InputBinder { get; }
    IActorTravelEndpoint Travel { get; }
    IActorDialogueEndpoint Dialogue { get; }

    IActorTargeting Targeting { get; }
}
```

Где:

```csharp
public interface IActorTargeting
{
    ITargetable CurrentTarget { get; }

    event Action<ITargetable> CurrentTargetChanged;

    void SetTarget(ITargetable target);
    void ClearTarget();
}
```

Можно оставить имя `ITargetProvider`, но я бы переименовал в `IActorTargeting`, потому что это не просто provider, а actor capability/state.

## Почему не реестр

Реестр `ITargetProvider` нужен только если какая-то внешняя система имеет **только `actorWorldId`** и хочет найти provider:

```text
actorWorldId -> ITargetProvider
```

Но если у тебя уже есть:

```text
actorWorldId -> IWorldActor
```

то лучше:

```csharp
if (_actors.TryGet(actorId, out var actor))
{
    var target = actor.Targeting.CurrentTarget;
}
```

А не:

```csharp
_targetProviders.TryGet(actorId, out var provider);
```

Иначе ты опять дробишь actor internals на глобальные registry.

## Важная оговорка

Если не каждый actor умеет target-lock, не заставляй всех иметь `Targeting`.

Тогда можно так:

```csharp
public interface IWorldActor
{
    WorldId WorldId { get; }
    ActorDefinition Definition { get; }

    IActorView View { get; }

    IActorInputBinder InputBinder { get; }
    IActorTravelEndpoint Travel { get; }
    IActorDialogueEndpoint Dialogue { get; }

    bool TryGetTargeting(out IActorTargeting targeting);
}
```

Но если в твоей игре почти все actors потенциально могут выбирать цель, проще держать:

```csharp
IActorTargeting Targeting { get; }
```

## Как это использовать в UI

Например player nameplate/lock-on UI:

```text
PlayerService.CurrentActor
  -> ActorManager.TryGet(actorId)
  -> actor.Targeting.CurrentTarget
  -> target.WorldId
  -> DisplayManager.GetDisplayName(target.WorldId)
```

UI не должен искать `ITargetProvider` в отдельном registry.

## Итог

Да: **`ITargetProvider` лучше положить внутрь `IWorldActor` как actor-specific capability**, а отдельный registry убрать.

Я бы сделал:

```text
ITargetable
  = объект, который можно выбрать как цель

IActorTargeting
  = состояние выбора цели у актора

IWorldActor.Targeting
  = доступ к actor targeting

Target registry
  = не нужен
```
