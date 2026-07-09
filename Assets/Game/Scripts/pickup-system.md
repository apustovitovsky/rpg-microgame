Я вообще думаю убрать все эти pickup effects, сделать ICollectable, им будут LootSource и Pickup. Притом они даже могут обращаться к одному и тому же менеджеру если это будет удобно, или к разным. Но главное что если я захочу чтобы ICollectable лечил здоровье, я могу просто добавить другую реализацию.

Да, это может быть **лучше для MVP**, чем `PickupEffect`-система.

Но я бы аккуратно разделил:

```text id="z5n3y0"
IInteractable = игрок нажал interact
ICollectable = объект можно "забрать/собрать/получить"
IInventoryOwner = у объекта есть inventory
ILootSource = из объекта можно лутать содержимое
```

## Хорошая схема

```csharp id="f1x64x"
public interface ICollectable
{
    WorldId WorldId { get; }

    bool CanCollect(WorldId collectorId);

    UniTask<CollectResult> CollectAsync(
        WorldId collectorId,
        CancellationToken token);
}
```

Тогда `PickupInteractable` становится универсальным адаптером:

```csharp id="ax302h"
public sealed class CollectableInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private MonoBehaviour _collectableBehaviour;

    private ICollectable _collectable;

    private void Awake()
    {
        _collectable = (ICollectable)_collectableBehaviour;
    }

    public WorldId WorldId => _collectable.WorldId;

    public bool CanInteract(InteractionContext context)
    {
        return _collectable.CanCollect(context.InteractorWorldId);
    }

    public UniTask InteractAsync(
        InteractionContext context,
        CancellationToken token)
    {
        return _collectable.CollectAsync(
            context.InteractorWorldId,
            token);
    }
}
```

И дальше разные реализации:

```text id="eyr4zq"
ItemPickupCollectable
HealingPickupCollectable
ManaPickupCollectable
GoldPickupCollectable
LootSourceCollectable
QuestItemCollectable
```

## Пример: обычный item pickup

```csharp id="auz57c"
public sealed class ItemPickupCollectable : MonoBehaviour, ICollectable
{
    [SerializeField] private ItemDefinition _item;
    [SerializeField] private int _amount = 1;

    private WorldId _worldId;
    private IInventoryService _inventoryService;
    private IWorldManager _worldManager;

    public WorldId WorldId => _worldId;

    public void Initialize(
        WorldId worldId,
        IInventoryService inventoryService,
        IWorldManager worldManager)
    {
        _worldId = worldId;
        _inventoryService = inventoryService;
        _worldManager = worldManager;
    }

    public bool CanCollect(WorldId collectorId)
    {
        return _inventoryService.CanAdd(collectorId, _item, _amount);
    }

    public async UniTask<CollectResult> CollectAsync(
        WorldId collectorId,
        CancellationToken token)
    {
        if (!CanCollect(collectorId))
            return CollectResult.Failed;

        await _inventoryService.AddAsync(collectorId, _item, _amount, token);

        _worldManager.Despawn(_worldId);

        return CollectResult.Succeeded;
    }
}
```

## Пример: healing pickup

```csharp id="5v706a"
public sealed class HealingCollectable : MonoBehaviour, ICollectable
{
    [SerializeField] private int _amount = 25;

    private WorldId _worldId;
    private IAttributeService _attributeService;
    private IWorldManager _worldManager;

    public WorldId WorldId => _worldId;

    public bool CanCollect(WorldId collectorId)
    {
        return _attributeService.CanRestoreHealth(collectorId, _amount);
    }

    public async UniTask<CollectResult> CollectAsync(
        WorldId collectorId,
        CancellationToken token)
    {
        if (!CanCollect(collectorId))
            return CollectResult.Failed;

        await _attributeService.RestoreHealthAsync(collectorId, _amount, token);

        _worldManager.Despawn(_worldId);

        return CollectResult.Succeeded;
    }
}
```

Вот это как раз твоя мысль: **хочешь pickup, который лечит — просто вешаешь другую реализацию `ICollectable`**.

## Что с сундуком

Сундук можно сделать `ICollectable`, но тут надо быть осторожным.

Если interact с сундуком означает:

```text id="mtwg19"
открыть окно лута
```

то это скорее:

```text id="jirt74"
LootSourceInteractable -> LootService.Open(...)
```

А не `ICollectable`.

Но если interact означает:

```text id="8d144l"
забрать всё из сундука сразу
```

тогда сундук может быть `ICollectable`:

```csharp id="r8tugy"
public sealed class LootSourceCollectable : MonoBehaviour, ICollectable
{
    private ILootSource _source;
    private ILootService _lootService;

    public WorldId WorldId => _source.WorldId;

    public bool CanCollect(WorldId collectorId)
    {
        return _source.CanLoot(collectorId);
    }

    public UniTask<CollectResult> CollectAsync(
        WorldId collectorId,
        CancellationToken token)
    {
        return _lootService.CollectAllAsync(
            collectorId,
            _source,
            token);
    }
}
```

То есть:

```text id="h0f0b8"
сундук с UI-лутом     -> ILootSource + LootSourceInteractable
сундук "забрать всё" -> ILootSource + ICollectable + CollectableInteractable
```

## Чем это лучше, чем PickupEffect

Плюсы:

```text id="l95ebw"
меньше абстракций
меньше registry
нет effect handlers
поведение видно прямо на prefab-е
легко сделать разные варианты pickup-а
```

Минусы:

```text id="ah4rrp"
меньше data-driven гибкости
для комбинаций эффектов нужно делать новые компоненты или composite collectable
```

Например, если предмет должен одновременно:

```text id="5w19o1"
дать золото
вылечить
запустить quest flag
проиграть VFX
```

то без effects тебе понадобится либо:

```text id="wjy9z9"
CompositeCollectable
```

либо отдельный компонент:

```text id="r677ll"
GoldHealQuestCollectable
```

Для MVP это нормально. Потом можно вернуть data-driven effects, если реально понадобится.

## Я бы сделал так сейчас

Убрать `PickupEffect`-систему и сделать:

```text id="bkohga"
ICollectable
CollectableInteractable
ItemPickupCollectable
HealingCollectable
GoldCollectable
LootSourceCollectable, если нужен collect-all
```

А отдельно оставить:

```text id="8phwv8"
IInventoryOwner
ILootSource
IInventoryService
ILootService
IAttributeService
```

Главное: `ICollectable` не должен сам означать “это pickup”. Это просто:

```text id="vvg8q8"
объект может быть собран collector-ом
```

Тогда и pickup, и труп, и сундук, и лечебная сфера могут использовать один interaction-адаптер, но иметь разные реализации поведения.

Я не имел в виду универсальный Interactable. Скорее разные связки ItemPickupInteractable + ItemPickupCollectable

Да, это как раз лучше, чем универсальный `CollectableInteractable`.

То есть не:

```text
CollectableInteractable -> ICollectable
```

как один общий adapter на всё, а конкретные пары:

```text
ItemPickupInteractable -> ItemPickupCollectable
HealingPickupInteractable -> HealingCollectable
LootSourceInteractable -> LootSource / LootCollectable
GoldPickupInteractable -> GoldCollectable
```

И это нормально.

## Почему конкретная связка лучше

Потому что `Interactable` — это **входной сценарий**, а `Collectable` — это **действие/модель сбора**.

Например:

```text
ItemPickupInteractable
  отвечает за interaction:
    range
    prompt text
    CanInteract
    InteractAsync

ItemPickupCollectable
  отвечает за collect:
    какой item
    сколько
    можно ли добавить в inventory
    добавить в inventory
    despawn
```

Это чище, чем один универсальный компонент, потому что у разных interactable могут быть разные UX/правила:

```text
ItemPickupInteractable    -> "Pick up Sword"
HealingPickupInteractable -> "Use Healing Orb"
ChestInteractable         -> "Open"
CorpseInteractable        -> "Loot"
```

Хотя технически все они могут внутри вызывать похожие сервисы.

## Примерная форма

```csharp
public interface ICollectable
{
    WorldId WorldId { get; }

    bool CanCollect(WorldId collectorId);

    UniTask<CollectResult> CollectAsync(
        WorldId collectorId,
        CancellationToken token);
}
```

```csharp
public sealed class ItemPickupInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemPickupCollectable _collectable;

    public WorldId WorldId => _collectable.WorldId;
    public Vector3 InteractionPosition => transform.position;
    public float MaxRange => 2f;

    public bool CanInteract(InteractionContext context)
    {
        return _collectable.CanCollect(context.InteractorWorldId);
    }

    public UniTask InteractAsync(
        InteractionContext context,
        CancellationToken token)
    {
        return _collectable.CollectAsync(
            context.InteractorWorldId,
            token);
    }
}
```

```csharp
public sealed class ItemPickupCollectable : MonoBehaviour, ICollectable
{
    [SerializeField] private ItemDefinition _item;
    [SerializeField] private int _amount = 1;

    private WorldId _worldId;
    private IInventoryService _inventoryService;
    private IWorldManager _worldManager;

    public WorldId WorldId => _worldId;

    public void Initialize(
        WorldId worldId,
        IInventoryService inventoryService,
        IWorldManager worldManager)
    {
        _worldId = worldId;
        _inventoryService = inventoryService;
        _worldManager = worldManager;
    }

    public bool CanCollect(WorldId collectorId)
    {
        return _inventoryService.CanAdd(collectorId, _item, _amount);
    }

    public async UniTask<CollectResult> CollectAsync(
        WorldId collectorId,
        CancellationToken token)
    {
        if (!CanCollect(collectorId))
            return CollectResult.Failed;

        await _inventoryService.AddAsync(collectorId, _item, _amount, token);

        _worldManager.Despawn(_worldId);

        return CollectResult.Succeeded;
    }
}
```

## Тогда pickup service может вообще исчезнуть

Если у тебя есть:

```text
ItemPickupCollectable
HealingCollectable
GoldCollectable
```

то отдельный `PickupService` становится не обязательным.

Вместо:

```text
PickupInteractable -> PickupService -> PickupEffectHandlers
```

будет:

```text
ItemPickupInteractable -> ItemPickupCollectable -> InventoryService
HealingPickupInteractable -> HealingCollectable -> AttributeService
GoldPickupInteractable -> GoldCollectable -> CurrencyService
```

Это проще и прозрачнее.

## Но сервисы всё равно нужны

Только не `PickupService`, а более предметные:

```text
InventoryService
AttributeService
CurrencyService
LootService
WorldManager
```

`Collectable` не должен сам копаться в чужом inventory напрямую, если есть нормальный сервис.

То есть хорошо:

```text
ItemPickupCollectable -> InventoryService.Add(...)
HealingCollectable -> AttributeService.Restore(...)
LootSourceCollectable -> LootService.TakeAll(...)
```

Плохо:

```text
ItemPickupCollectable сам ищет Actor, сам меняет List<ItemStack>
```

## Для сундука

Сундук я бы не делал `ItemPickupCollectable`.

У него своя связка:

```text
ChestInteractable -> ChestLootSource
```

или если кнопка “забрать всё”:

```text
ChestCollectAllInteractable -> LootSourceCollectable
```

То есть:

```text
обычный item в мире:
  ItemPickupInteractable + ItemPickupCollectable

лечилка в мире:
  HealingPickupInteractable + HealingCollectable

сундук:
  ChestInteractable + ChestLootSource

труп:
  CorpseInteractable + CorpseLootSource
```

## Главный плюс твоей идеи

Ты убираешь слой:

```text
PickupDefinition.Effects -> handlers -> provider
```

и заменяешь его на композицию prefab-а:

```text
какой collectable-компонент повесил,
такое поведение и получил
```

Для Unity/MVP это очень нормальный подход.

## Мой вердикт

Да, я бы делал именно так:

```text
не универсальный Interactable
а конкретные пары:
  ItemPickupInteractable + ItemPickupCollectable
  HealingPickupInteractable + HealingCollectable
  GoldPickupInteractable + GoldCollectable
  ChestInteractable + LootSource
```

`ICollectable` оставить как общий контракт можно, но не обязательно регистрировать его глобально. Он нужен скорее как локальный контракт между конкретным `Interactable` и конкретным `Collectable`.
