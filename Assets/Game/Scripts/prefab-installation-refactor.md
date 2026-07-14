Да, пара **`IPrefabInstaller` + `PrefabScope`** выглядит достаточно чисто:

```text
PrefabScope
    только находит локальные installers
    и передаёт им IContainerBuilder

IPrefabInstaller
    регистрирует feature, представленную endpoint-компонентом
```

VContainer уже использует `IInstaller` как стандартный способ добавить registrations в создаваемый `LifetimeScope`, а сам контейнер после построения неизменяем. Поэтому registrations действительно нужно собрать внутри `Configure`, до завершения построения scope. ([vcontainer.hadashikick.jp][1])

## Интерфейс

```csharp
using VContainer;

namespace Game.Gameplay
{
    public interface IPrefabInstaller : IInstaller
    {
    }
}
```

Marker-интерфейс нужен, чтобы `PrefabScope` выбирал только предназначенные для DI компоненты, а не любой `MonoBehaviour`.

## Scope

```csharp
using System;
using System.Linq;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Gameplay
{
    public sealed class PrefabScope : LifetimeScope
    {
        [SerializeField]
        private Transform _compositionRoot;

        protected override void Configure(
            IContainerBuilder builder)
        {
            if (_compositionRoot == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(PrefabScope)} has no composition root.");
            }

            var installers = _compositionRoot
                .GetComponentsInChildren<MonoBehaviour>(true)
                .OfType<IPrefabInstaller>()
                .ToArray();

            foreach (var installer in installers)
            {
                installer.Install(builder);
            }
        }
    }
}
```

`PrefabScope` не обязан располагаться на корневом `GameObject`. `_compositionRoot` задаёт границу конкретного prefab вручную.

Не стоит использовать:

```csharp
transform.root
```

После помещения объекта под runtime-parent это может оказаться корнем всей сцены или чужой иерархии.

## Endpoint

Например, inventory:

```csharp
using UnityEngine;
using VContainer;

namespace Game.Inventory
{
    public sealed class InventoryEndpoint :
        MonoBehaviour,
        IPrefabInstaller
    {
        public void Install(
            IContainerBuilder builder)
        {
            builder.RegisterComponent(this);

            builder.Register<IInventory>(
                resolver =>
                {
                    var instance =
                        resolver.Resolve<ActorInstance>();

                    var fragment =
                        instance.Definition
                            .GetRequiredFragment<
                                InventoryFragment>();

                    var factory =
                        resolver.Resolve<IInventoryFactory>();

                    return factory.Create(fragment);
                },
                Lifetime.Scoped);

            builder.RegisterEntryPoint<
                InventoryRegistration>();
        }
    }
}
```

Targeting:

```csharp
public sealed class TargetEndpoint :
    MonoBehaviour,
    ITargetProvider,
    IPrefabInstaller
{
    [SerializeField]
    private Transform _anchor;

    public Transform Anchor => _anchor;

    public void Install(
        IContainerBuilder builder)
    {
        builder.RegisterComponent(this)
            .As<ITargetProvider>();

        builder.RegisterEntryPoint<
            TargetProviderRegistration>();
    }
}
```

VContainer позволяет регистрировать уже найденный экземпляр `MonoBehaviour` через `RegisterComponent`, а `Enqueue(IInstaller)` предназначен для внешних registrations следующего создаваемого scope — например, передачи конкретного runtime instance. ([vcontainer.hadashikick.jp][1])

## Spawn

Снаружи передаётся только instance:

```csharp
var installer =
    new ActorInstanceInstaller(instance);

using (LifetimeScope.EnqueueParent(_parentScope))
using (LifetimeScope.Enqueue(installer))
{
    var gameObject = Object.Instantiate(
        definition.Prefab,
        position,
        rotation,
        parent);
}
```

Дальше:

```text
внешний installer
    регистрирует ActorInstance

PrefabScope
    находит InventoryEndpoint
    находит TargetEndpoint
    находит InteractionEndpoint
    находит CommandReceiverEndpoint

каждый endpoint
    регистрирует собственную feature
```

## Одно важное правило

`PrefabScope` не должен сканировать область дочернего `PrefabScope`. Иначе installer вложенного объекта будет зарегистрирован дважды — в родительском и собственном контейнере.

Практичнее сразу фильтровать endpoints по ближайшему scope:

```csharp
private IPrefabInstaller[] FindInstallers()
{
    return _compositionRoot
        .GetComponentsInChildren<MonoBehaviour>(true)
        .OfType<IPrefabInstaller>()
        .Where(installer =>
        {
            var component = (MonoBehaviour)installer;

            return component.GetComponentInParent<PrefabScope>(
                       true) == this;
        })
        .ToArray();
}
```

Так каждый endpoint принадлежит ближайшему `PrefabScope`.

## Названия

Я бы оставил именно:

```text
PrefabScope
IPrefabInstaller
```

Они точно передают роли:

* `PrefabScope` — VContainer scope композиции одного prefab;
* `IPrefabInstaller` — компонент, добавляющий registrations этого prefab.

А сами компоненты лучше называть по feature:

```text
InventoryEndpoint
TargetEndpoint
InteractionEndpoint
NavigationEndpoint
CommandReceiverEndpoint
```

Итоговая модель:

```text
Definition fragments
    данные feature

Endpoint
    Unity-ссылки + VContainer registration feature

PrefabScope
    автоматический сбор endpoints

Spawner
    только runtime instance + instantiate/despawn
```

Это убирает большие `ActorModule`/`PickupScope`, но не создаёт нового универсального контейнера бизнес-логики.

[1]: https://vcontainer.hadashikick.jp/scoping/generate-child-via-scene?utm_source=chatgpt.com "Generate child scope via scene or prefab - VContainer"

# Prefab Installation Refactor

## Summary
Перевести композицию `PF_Actor`, `PF_Chest` и `PF_Pickup` с `ActorModule` / `PickupScope` / `ModuleBuilder`-ассетов на prefab-driven DI:

```text
WorldSpawner
    → Enqueue(instance installer)
    → Instantiate prefab
    → PrefabScope собирает локальные IPrefabInstaller
    → endpoint регистрирует только свою feature
```

`PrefabScope` и `IPrefabInstaller` находятся в `Game.Core`.

## Core Composition
- Добавить marker `IPrefabInstaller : IInstaller`.
- Добавить `PrefabScope : LifetimeScope` с явным `_compositionRoot`.
- В `Configure` scope находит активные и неактивные `MonoBehaviour`, реализующие `IPrefabInstaller`.
- Installer принадлежит scope, только если его ближайший `PrefabScope` равен текущему; вложенные scope не регистрируются родительским.
- `WorldSpawner` сохраняет текущую схему `EnqueueParent` + `Enqueue(new ...InstanceInstaller(instance))`; его ответственность не расширяется.
- Удалить `ModuleRoot`, `ModuleBuilder`, `RegisterComponentInModuleRoot`, `ActorModule`, `PickupScope` и все module-builder/configurator assets после миграции prefab.

## Endpoint Composition
- `Targetable` → `TargetEndpoint`: сам регистрирует себя как `ITargetable`.
- `InventoryOwner` → `InventoryEndpoint`: создаёт `InventoryInstance` из fragment, публикует его через существующий `RegistryBinding<InventoryInstance>`.
- `DialogueParticipant` → `DialogueEndpoint`: регистрирует себя как `IInteractable` и `IDialogueSessionStarter`; добавляет `InteractCommandHandler`.
- `LootInteractable` → `LootInteractionEndpoint`: регистрирует себя как `IInteractable`; добавляет `InteractCommandHandler`.
- `ItemPickupCollectable` → `ItemPickupEndpoint`: регистрирует себя как `ICollectable`.
- `ItemPickupInteractable` → `ItemPickupInteractionEndpoint`: регистрирует себя как `IInteractable`; добавляет `InteractCommandHandler`.
- `PossessionEndpoint` остаётся endpoint: регистрирует себя как `IPossessionEndpoint`, `ActorInputBinder` и `PossessCommandHandler`.
- `ActorLookController`, `MovementController`, `ActorTargetController`, `NavMeshPlannerEndpoint`, `NavMeshTravelEndpoint` и нужные AI-компоненты получают локальную self-registration через `IPrefabInstaller`, сохраняя свои controller-названия и runtime-поведение.
- AI endpoint заменяет `ActorAIConfiguratorSO`: локально регистрирует `NavMeshAgent`, planner, input и связывает AI-input с look/movement без module asset.

## Commands
- Добавить `CommandReceiverEndpoint` на prefab, который принимает world-команды.
- Он регистрирует `WorldCommandReceiver` и `RegistryBinding<ICommandReceiver>`, но не знает конкретных handlers.
- Каждый feature-endpoint добавляет только собственные handlers: interaction добавляет `InteractCommandHandler`, possession добавляет `PossessCommandHandler`.
- `CommandManager` получает собственный `Dictionary<Guid, ICommandReceiver>`, реализует `ICommandManager` и `IRegistryWriter<ICommandReceiver>`.
- Удалить глобальный `Registry<ICommandReceiver>` и регистрацию этого registry из `CommandsModuleBuilder`; `RegistryBinding<ICommandReceiver>` пишет прямо в `CommandManager`.

## Prefab Migration And Checks
- `PF_Actor`: `PrefabScope`, target, inventory, dialogue, possession, command receiver, control и нужные AI endpoints.
- `PF_Chest`: `PrefabScope`, target, inventory, loot interaction и command receiver; без possession/control endpoint.
- `PF_Pickup`: `PrefabScope`, target, item pickup и item-pickup interaction endpoints, command receiver.
- Убрать прежние build callbacks, которые принудительно `Resolve<Targetable>`: `RegisterComponent(this)` сам выполняет injection компонента.
- Проверить spawn/despawn всех трёх prefab, регистрацию и удаление inventory/command receiver, interact для pickup, loot для chest, possession для actor и AI navigation.
- Отдельно проверить prefab с вложенным `PrefabScope`: endpoint внутреннего scope не должен попасть в контейнер внешнего.

## Assumptions
- На одном prefab допускается только один `IInteractable` и один `ICommandReceiver`.
- Каждая feature остаётся отдельным endpoint-компонентом; универсальный interaction-компонент со ссылками на реализации не вводится.
- Миграция выполняется целиком без сохранения compatibility-мостов и старых module assets.
