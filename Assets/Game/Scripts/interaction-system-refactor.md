Да. Тогда `InteractionService` — **владелец собственного индекса interactable-объектов** и одновременно `IRegistryWriter<IInteractable>`. Никакого отдельного `Registry<IInteractable>` снаружи ему не передаётся.

Итоговый поток:

```text
InteractCommand(TargetInstanceId)
    ↓ адресуется CommandReceiver инициатора
InteractCommandHandler
    ↓
InteractionService
    ↓ внутренний InstanceId → IInteractable
IInteractable.InteractAsync(...)
```

## 1. Команда

Обычный класс, без `record struct`:

```csharp
using System;
using Game.Commands;

namespace Game.Interaction
{
    public sealed class InteractCommand : IWorldCommand
    {
        public InteractCommand(Guid targetInstanceId)
        {
            TargetInstanceId = targetInstanceId;
        }

        public Guid TargetInstanceId { get; }
    }
}
```

Команда отправляется receiver того, кто выполняет действие:

```csharp
_commandDispatcher.DispatchAsync(
    interactorInstanceId,
    new InteractCommand(targetInstanceId),
    cancellationToken);
```

## 2. `IInteractor` без identity

```csharp
using UnityEngine;

namespace Game.Interaction
{
    public interface IInteractor
    {
        Vector3 InteractionOrigin { get; }
    }
}
```

`InstanceId` сюда не добавляем. ID инициатора уже известен `WorldCommandReceiver` и передаётся handler через его стандартный контекст.

## 3. `InteractionService` хранит interactables сам

Названия методов writer подставь из своего Core Registry API, но ответственность должна выглядеть так:

```csharp
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Interaction
{
    public sealed class InteractionService :
        IInteractionService,
        IRegistryWriter<IInteractable>
    {
        private readonly Dictionary<Guid, IInteractable> _interactables =
            new Dictionary<Guid, IInteractable>();

        public void Add(
            Guid instanceId,
            IInteractable interactable)
        {
            if (interactable == null)
            {
                throw new ArgumentNullException(
                    nameof(interactable));
            }

            if (!_interactables.TryAdd(
                    instanceId,
                    interactable))
            {
                throw new InvalidOperationException(
                    $"Interactable '{instanceId}' is already registered.");
            }
        }

        public void Remove(
            Guid instanceId,
            IInteractable interactable)
        {
            if (!_interactables.TryGetValue(
                    instanceId,
                    out var registered))
            {
                return;
            }

            // Защита от старого binding, который пытается удалить
            // новую регистрацию с таким же ID.
            if (!ReferenceEquals(registered, interactable))
            {
                return;
            }

            _interactables.Remove(instanceId);
        }

        public async UniTask<InteractionResult> InteractAsync(
            InteractionContext context,
            CancellationToken cancellationToken)
        {
            if (context.InteractorInstanceId == Guid.Empty ||
                context.TargetInstanceId == Guid.Empty)
            {
                return InteractionResult.Invalid;
            }

            if (context.InteractorInstanceId ==
                context.TargetInstanceId)
            {
                return InteractionResult.Invalid;
            }

            if (!_interactables.TryGetValue(
                    context.TargetInstanceId,
                    out var interactable))
            {
                return InteractionResult.NotFound;
            }

            var distance = Vector3.Distance(
                context.Origin,
                interactable.InteractionPoint);

            if (distance > interactable.InteractionRange)
            {
                return InteractionResult.OutOfRange;
            }

            if (!interactable.CanInteract(context))
            {
                return InteractionResult.Rejected;
            }

            cancellationToken.ThrowIfCancellationRequested();

            await interactable.InteractAsync(
                context,
                cancellationToken);

            return InteractionResult.Completed;
        }
    }
}
```

Если у тебя уже есть `WorldIndex<T>` или похожая коллекция в Core, внутри можно использовать её:

```csharp
private readonly WorldIndex<IInteractable> _interactables = new();
```

Но это **внутренняя деталь `InteractionService`**, а не внедряемый внешний registry.

## 4. Контракт сервиса

```csharp
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Interaction
{
    public interface IInteractionService
    {
        UniTask<InteractionResult> InteractAsync(
            InteractionContext context,
            CancellationToken cancellationToken);
    }
}
```

Контекст остаётся orchestration DTO и спокойно содержит ID:

```csharp
using System;
using UnityEngine;

namespace Game.Interaction
{
    public readonly struct InteractionContext
    {
        public InteractionContext(
            Guid interactorInstanceId,
            Vector3 origin,
            Guid targetInstanceId)
        {
            InteractorInstanceId = interactorInstanceId;
            Origin = origin;
            TargetInstanceId = targetInstanceId;
        }

        public Guid InteractorInstanceId { get; }

        public Vector3 Origin { get; }

        public Guid TargetInstanceId { get; }
    }
}
```

Это не capability-протокол, поэтому наличие `Guid` здесь нормально.

## 5. Тонкий handler на стороне инициатора

```csharp
using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Interaction
{
    public sealed class InteractCommandHandler :
        IWorldCommandHandler<InteractCommand>
    {
        private readonly IInteractor _interactor;
        private readonly IInteractionService _interactionService;

        public InteractCommandHandler(
            IInteractor interactor,
            IInteractionService interactionService)
        {
            _interactor = interactor;
            _interactionService = interactionService;
        }

        public async UniTask<CommandResult> HandleAsync(
            Guid receiverInstanceId,
            InteractCommand command,
            CancellationToken cancellationToken)
        {
            var context = new InteractionContext(
                receiverInstanceId,
                _interactor.InteractionOrigin,
                command.TargetInstanceId);

            var result =
                await _interactionService.InteractAsync(
                    context,
                    cancellationToken);

            return ToCommandResult(result);
        }

        private static CommandResult ToCommandResult(
            InteractionResult result)
        {
            switch (result)
            {
                case InteractionResult.Completed:
                    return CommandResult.Completed;

                case InteractionResult.Cancelled:
                    return CommandResult.Cancelled;

                default:
                    return CommandResult.Rejected;
            }
        }
    }
}
```

Handler не:

* ищет target;
* проверяет дистанцию;
* вызывает `CanInteract`;
* содержит registry logic.

Он только переводит command context в interaction context.

## 6. Endpoint инициатора

```csharp
using UnityEngine;
using VContainer;

namespace Game.Interaction
{
    public sealed class InteractorEndpoint :
        MonoBehaviour,
        IInteractor,
        IPrefabInstaller
    {
        [SerializeField]
        private Transform _interactionOrigin;

        public Vector3 InteractionOrigin =>
            _interactionOrigin.position;

        public void Install(IContainerBuilder builder)
        {
            builder.RegisterComponent(this)
                .As<IInteractor>();

            builder.Register<InteractCommandHandler>(
                    Lifetime.Scoped)
                .As<IWorldCommandHandler>();
        }
    }
}
```

На actor prefab:

```text
CommandReceiverEndpoint
InteractorEndpoint
```

## 7. Interactable регистрируется непосредственно в сервисе

Глобальная DI-регистрация:

```csharp
builder.Register<InteractionService>(Lifetime.Singleton)
    .As<IInteractionService>()
    .As<IRegistryWriter<IInteractable>>();
```

Твой существующий generic `RegistryBinding<IInteractable>` получит именно этот writer.

На сундуке или pickup:

```csharp
public sealed class LootInteractionEndpoint :
    MonoBehaviour,
    IInteractable,
    IRegistryBindingSource<IInteractable>,
    IPrefabInstaller
{
    private IWorldIdentity _identity;

    public Guid Id => _identity.InstanceId;

    public IInteractable Value => this;

    public void Install(IContainerBuilder builder)
    {
        builder.RegisterComponent(this)
            .As<IInteractable>()
            .As<IRegistryBindingSource<IInteractable>>();

        builder.RegisterEntryPoint<
            RegistryBinding<IInteractable>>();
    }
}
```

Здесь `Guid` находится в специальном инфраструктурном `IRegistryBindingSource<T>`, а не загрязняет `IInteractable`.

## Что удаляется с сундуков и pickup

```text
CommandReceiverEndpoint
InteractCommandHandler
RegistryBinding<ICommandReceiver>
```

Остаётся:

```text
LootInteractionEndpoint / PickupInteractionEndpoint
RegistryBinding<IInteractable>
```

## Ответственность после рефактора

```text
CommandReceiver
    исполняет команды инициатора

InteractCommandHandler
    адаптирует команду к interaction use case

InteractionService
    хранит interactables
    является IRegistryWriter<IInteractable>
    находит цель
    проверяет общие правила
    запускает взаимодействие

IInteractable
    реализует конкретное действие сундука, pickup, двери и т. п.
```

Это соответствует твоей текущей модели реестров: **feature service сам владеет индексом и предоставляет writer для generic bindings других scope**.

Да, точная модель такая:

> `InteractionService` **владеет `Registry<IInteractable>`**, использует его для чтения и проксирует его writer API наружу.

Твой `Registry<T>` уже реализует и `IRegistry<T>`, и `IRegistryWriter<T>`, включая проверки ID, дубликатов и безопасное удаление через `expectedValue`. `RegistryBinding<T>` при старте вызывает `writer.Add(...)`, а при уничтожении scope — `writer.Remove(...)`. ([GitHub][1])

## `InteractionService`

```csharp
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core;
using UnityEngine;

namespace Game.Interaction
{
    public sealed class InteractionService :
        IInteractionService,
        IRegistryWriter<IInteractable>
    {
        private readonly Registry<IInteractable> _interactables = new();

        public void Add(
            Guid id,
            IInteractable value)
        {
            _interactables.Add(id, value);
        }

        public bool Remove(
            Guid id,
            IInteractable expectedValue)
        {
            return _interactables.Remove(
                id,
                expectedValue);
        }

        public async UniTask<bool> TryInteractAsync(
            InteractionContext context,
            CancellationToken token)
        {
            if (context.InteractorInstanceId == Guid.Empty ||
                context.TargetInstanceId == Guid.Empty ||
                context.InteractorInstanceId ==
                context.TargetInstanceId)
            {
                return false;
            }

            if (!_interactables.TryGet(
                    context.TargetInstanceId,
                    out var interactable))
            {
                return false;
            }

            var distance = Vector3.Distance(
                context.Origin,
                interactable.InteractionPoint);

            if (distance > interactable.MaxRange)
            {
                return false;
            }

            if (!interactable.CanInteract(context))
            {
                return false;
            }

            token.ThrowIfCancellationRequested();

            await interactable.InteractAsync(
                context,
                token);

            return true;
        }
    }
}
```

То есть внутри сервиса нет:

```csharp
Dictionary<Guid, IInteractable>
```

и нет внедряемого:

```csharp
IRegistry<IInteractable>
IRegistryWriter<IInteractable>
```

Он сам создаёт конкретный Core registry:

```csharp
private readonly Registry<IInteractable> _interactables = new();
```

## Контракт

```csharp
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Interaction
{
    public interface IInteractionService
    {
        UniTask<bool> TryInteractAsync(
            InteractionContext context,
            CancellationToken token);
    }
}
```

## Глобальная регистрация

```csharp
builder.Register<InteractionService>(
        Lifetime.Singleton)
    .As<IInteractionService>()
    .As<IRegistryWriter<IInteractable>>();
```

Принципиально наружу не публикуется:

```csharp
IRegistry<IInteractable>
```

Читать interactables может только `InteractionService`. Другим объектам доступен лишь writer, необходимый `RegistryBinding<IInteractable>`.

## Interactable на prefab

```csharp
public void Install(IContainerBuilder builder)
{
    builder.RegisterComponent(this)
        .As<IInteractable>();

    builder.RegisterEntryPoint<
        RegistryBinding<IInteractable>>(
        Lifetime.Scoped);
}
```

Существующий `RegistryBinding<IInteractable>` получит:

```text
IInstanceIdentity
IInteractable
IRegistryWriter<IInteractable>
```

А writer будет указывать на `InteractionService`, который проксирует регистрацию в свой `_interactables`. Текущий generic binding уже рассчитан именно на такую схему. ([GitHub][2])

## Команда после переноса receiver к инициатору

Совместимый с текущим стилем обычный `readonly struct`:

```csharp
using System;
using Game.Commands;
using UnityEngine;

namespace Game.Interaction
{
    public readonly struct InteractCommand :
        IWorldCommand
    {
        public InteractCommand(
            Guid targetInstanceId,
            Vector3 interactorPosition)
        {
            TargetInstanceId = targetInstanceId;
            InteractorPosition = interactorPosition;
        }

        public Guid TargetInstanceId { get; }

        public Vector3 InteractorPosition { get; }
    }
}
```

Команда адресуется receiver инициатора:

```csharp
_dispatcher.DispatchAsync(
    interactorInstanceId,
    new InteractCommand(
        targetInstanceId,
        interactorPosition),
    token);
```

## Тонкий handler

```csharp
public sealed class InteractCommandHandler :
    WorldCommandHandler<InteractCommand>
{
    private readonly IInteractionService _interactionService;

    public InteractCommandHandler(
        IInteractionService interactionService)
    {
        _interactionService = interactionService;
    }

    public override async UniTask<CommandResult> HandleAsync(
        InteractCommand command,
        Guid interactorInstanceId,
        CancellationToken token)
    {
        var context = new InteractionContext(
            interactorInstanceId,
            command.InteractorPosition,
            command.TargetInstanceId);

        try
        {
            var succeeded =
                await _interactionService.TryInteractAsync(
                    context,
                    token);

            return succeeded
                ? CommandResult.Completed
                : CommandResult.Rejected;
        }
        catch (OperationCanceledException)
        {
            return CommandResult.Cancelled;
        }
    }
}
```

Текущий handler сам получает локальный `IInteractable` и выполняет distance/availability checks, из-за чего он должен находиться на prefab цели. После рефактора он получает `IInteractionService`, а проверки и поиск цели переходят в сервис. ([GitHub][3])

## Итоговая структура

```text
InteractionService
    owns Registry<IInteractable>
    reads Registry<IInteractable>
    proxies IRegistryWriter<IInteractable>
    выполняет interaction policy

RegistryBinding<IInteractable>
    регистрирует prefab capability
    через InteractionService writer

InteractCommandHandler
    находится у инициатора
    создаёт InteractionContext
    вызывает InteractionService

Chest / Pickup
    имеют IInteractable
    не имеют CommandReceiver

Actor
    имеет CommandReceiver
    имеет InteractCommandHandler
```

Это именно та граница ответственности, которую ты описываешь: `Registry<T>` остаётся готовой Core-структурой хранения, а feature-service является её владельцем и единственной публичной точкой записи и использования.

[1]: https://github.com/apustovitovsky/rpg-microgame/blob/main/Assets/Game/Scripts/Core/Runtime/Registry/Registry.cs "rpg-microgame/Assets/Game/Scripts/Core/Runtime/Registry/Registry.cs at main · apustovitovsky/rpg-microgame · GitHub"
[2]: https://github.com/apustovitovsky/rpg-microgame/blob/main/Assets/Game/Scripts/Core/Runtime/Registry/RegistryBinding.cs "rpg-microgame/Assets/Game/Scripts/Core/Runtime/Registry/RegistryBinding.cs at main · apustovitovsky/rpg-microgame · GitHub"
[3]: https://github.com/apustovitovsky/rpg-microgame/blob/main/Assets/Game/Scripts/Interaction/Runtime/InteractCommandHandler.cs "rpg-microgame/Assets/Game/Scripts/Interaction/Runtime/InteractCommandHandler.cs at main · apustovitovsky/rpg-microgame · GitHub"
