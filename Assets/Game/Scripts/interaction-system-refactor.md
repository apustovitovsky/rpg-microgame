Проверю актуальную реализацию interaction целиком: input/targeting → command → receiver/handler → interactable capability, а также связанные registry bindings и prefab installers. Затем укажу конкретные архитектурные проблемы и минимальный рефактор.


Да, ощущение верное: сейчас interaction-система получилась **параллельной command-системе**, хотя по смыслу должна быть её локальной capability.

Сейчас поток примерно такой:

```text
InteractCommand
→ CommandReceiver
→ handler
→ InteractionService
→ собственный WorldIndex<IInteractable>
→ IInteractable
```

`InteractionService` одновременно содержит отдельный реестр interactable-объектов, выполняет адресацию по target ID, проверяет дистанцию и запускает действие. При этом адресация цели уже выполнена `CommandReceiver`. Получается второй routing layer и второй registry одного и того же world object. ([GitHub][1])

## Главная проблема

После введения `CommandReceiver` глобальный `InteractionService` больше не должен искать target:

```csharp
_interactables.TryGet(context.TargetInstanceId, out var interactable);
```

Receiver уже найден по `TargetInstanceId`, а его локальный handler должен получить локальный `IInteractable` через scope:

```text
CommandDispatcher
→ receiver конкретного prefab
→ InteractCommandHandler
→ локальный IInteractable
```

То есть interaction — не глобальная адресная система, а локальный use case объекта.

## Как я бы перестроил

### 1. Удалить interaction registry

Удалить:

```text
IInteractionRegistrationService
InteractionService._interactables
RegisterInteractable(...)
RegistryBinding<IInteractable>
```

`IInteractable` не требуется публиковать глобально, потому что снаружи объект адресуется через `ICommandReceiver`.

### 2. Сделать один локальный handler

В `Game.Interaction`:

```csharp
public sealed class InteractCommandHandler
    : WorldCommandHandler<InteractCommand>
{
    private readonly IInteractable _interactable;

    public InteractCommandHandler(IInteractable interactable)
    {
        _interactable = interactable;
    }

    protected override async UniTask<CommandResult> ExecuteAsync(
        InteractCommand command,
        CancellationToken token)
    {
        var context = new InteractionContext(
            command.InteractorInstanceId,
            command.Origin);

        var result = await _interactable.InteractAsync(
            context,
            token);

        return result.Succeeded
            ? CommandResult.Succeeded
            : CommandResult.Rejected;
    }
}
```

Каждая interaction-capability регистрирует:

```csharp
builder.RegisterComponent(this)
    .As<IInteractable>();

builder.Register<InteractCommandHandler>(Lifetime.Scoped)
    .As<IWorldCommandHandler>();
```

Тогда наличие `LootInteractionEndpoint` автоматически добавляет поддержку `InteractCommand`.

## Упростить `InteractionContext`

Сейчас context содержит и interactor, и target:

```csharp
InteractorInstanceId
Origin
TargetInstanceId
```

Но target уже определён receiver’ом. Локальный capability не должен повторно проверять:

```csharp
context.TargetInstanceId == _instance.InstanceId
```

Оставить:

```csharp
public readonly record struct InteractionContext(
    Guid InteractorInstanceId,
    Vector3 Origin);
```

Это уберёт повторяющиеся проверки из `LootInteractionEndpoint` и `ItemPickupInteractionEndpoint`. Сейчас оба endpoint самостоятельно перепроверяют target identity, хотя маршрутизация уже должна гарантировать принадлежность команды этому scope. ([GitHub][2])

## `IInteractable` сейчас скрывает ошибки

Текущий контракт:

```csharp
bool CanInteract(InteractionContext context);

UniTask InteractAsync(
    InteractionContext context,
    CancellationToken token);
```

имеет слабое место: `InteractAsync` ничего не возвращает. Например, loot endpoint может:

* не открыть сессию;
* обнаружить другую открытую сессию;
* не получить snapshot;
* не забрать предметы;

но просто записывает warning и возвращает completed task. Внешний `InteractionService` после этого считает interaction успешным. ([GitHub][2])

Лучше:

```csharp
public interface IInteractable
{
    Vector3 InteractionPoint { get; }

    float MaxRange { get; }

    InteractionAvailability GetAvailability(
        InteractionContext context);

    UniTask<InteractionResult> InteractAsync(
        InteractionContext context,
        CancellationToken token);
}
```

Либо для MVP ещё проще:

```csharp
public interface IInteractable
{
    Vector3 InteractionPoint { get; }

    float MaxRange { get; }

    UniTask<InteractionResult> TryInteractAsync(
        InteractionContext context,
        CancellationToken token);
}
```

Второй вариант избегает расхождения:

```text
CanInteract вернул true
→ состояние изменилось
→ InteractAsync уже выполнить нельзя
```

Для UI-подсказок позднее можно отдельно добавить read-only availability query.

## Где проверять дистанцию

Не в loot и не в pickup. Это общая interaction policy, поэтому её должен проверять `InteractCommandHandler` или локальный `InteractionExecutor`:

```csharp
var distance = Vector3.Distance(
    context.Origin,
    _interactable.InteractionPoint);

if (distance > _interactable.MaxRange)
    return CommandResult.OutOfRange;
```

При этом `LootInteractionEndpoint` отвечает только за loot-семантику, а `ItemPickupInteractionEndpoint` — за сбор pickup.

## Что не так с самими endpoint

### `LootInteractionEndpoint`

Сейчас один MonoBehaviour:

* является Unity endpoint;
* валидирует общие interaction invariants;
* открывает loot session;
* получает snapshot;
* забирает всё содержимое;
* форматирует debug log.

Это слишком много ответственности. ([GitHub][2])

Минимально оставить ему:

```csharp
public UniTask<InteractionResult> TryInteractAsync(...)
{
    var result = _sessions.TryOpen(
        context.InteractorInstanceId,
        _instance.InstanceId);

    return UniTask.FromResult(Map(result));
}
```

`TryTakeAll` вообще не должен автоматически происходить при открытии сундука, если предполагается loot UI. Interaction открывает session; UI или отдельная команда забирает предметы.

### `ItemPickupInteractionEndpoint`

Он выглядит лучше: interaction адаптирует общую команду к `IItemPickupService.CollectAsync`. Но `CanInteract` сейчас дублируется:

```text
ItemPickupInteractionEndpoint.CanInteract
→ ItemPickupEndpoint.CanCollect
→ ItemPickupService.CollectAsync
→ вероятно повторная проверка
```

Достаточно одного выполнения с результатом:

```csharp
public async UniTask<InteractionResult> TryInteractAsync(...)
{
    var result = await _pickupService.CollectAsync(
        context.InteractorInstanceId,
        _pickup,
        token);

    return result == CollectResult.Succeeded
        ? InteractionResult.Succeeded
        : InteractionResult.Rejected;
}
```

## Рекомендуемая итоговая цепочка

```text
Player / AI
    ↓
InteractCommand
    ↓
CommandDispatcher
    ↓ InstanceId
WorldCommandReceiver
    ↓ локальная коллекция handlers
InteractCommandHandler
    ↓ общая проверка range/cancellation
IInteractable
    ↓ feature-specific действие
Loot session / Pickup collection / Dialogue
```

Глобально публикуется только:

```text
InstanceId → ICommandReceiver
```

Локально в prefab scope находятся:

```text
IInteractable
InteractCommandHandler
```

## Конкретный рефактор

1. Удалить собственный registry из `InteractionService`.
2. Удалить `IInteractionRegistrationService`.
3. Превратить `InteractionService` в локальный `InteractCommandHandler` либо полностью удалить.
4. Убрать `TargetInstanceId` из `InteractionContext`.
5. Сделать interaction возвращающим `InteractionResult`.
6. Перенести общие проверки дистанции и cancellation в handler.
7. Оставить feature-specific проверки внутри loot/pickup.
8. Убрать автоматический `TryTakeAll` из открытия сундука.
9. Не публиковать `IInteractable` через Core registry.

Итог: **command system отвечает за адресацию, interaction handler — за общую политику взаимодействия, конкретный endpoint — только за feature-specific действие**. Сейчас эти три уровня частично смешаны.

[1]: https://raw.githubusercontent.com/apustovitovsky/rpg-microgame/main/Assets/Game/Scripts/Interaction/Runtime/InteractionService.cs "raw.githubusercontent.com"
[2]: https://raw.githubusercontent.com/apustovitovsky/rpg-microgame/main/Assets/Game/Scripts/Loot/Components/LootInteractionEndpoint.cs "raw.githubusercontent.com"
