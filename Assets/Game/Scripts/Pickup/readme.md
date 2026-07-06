Да, но я бы назвал не `ICollectable`, а **`IPickup`** или **`ICollectible`** — с уточнением роли.

Разница:

```text
IInteractable = объект можно использовать через interaction
IPickup / ICollectible = объект можно забрать в инвентарь / подобрать
```

Лучше не смешивать.

## Практичная модель

```csharp
public interface IInteractable
{
    bool CanInteract(InteractionContext context);

    UniTask<InteractionResult> InteractAsync(
        InteractionContext context,
        CancellationToken token);
}
```

```csharp
public interface IPickup
{
    bool CanPickup(PickupContext context);

    UniTask<PickupResult> PickupAsync(
        PickupContext context,
        CancellationToken token);
}
```

А `PickupInteractable` просто адаптирует interaction в pickup:

```csharp
public sealed class PickupInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private PickupObject _pickup;

    public bool CanInteract(InteractionContext context)
    {
        return _pickup.CanPickup(new PickupContext(context.Actor));
    }

    public async UniTask<InteractionResult> InteractAsync(
        InteractionContext context,
        CancellationToken token)
    {
        var result = await _pickup.PickupAsync(
            new PickupContext(context.Actor),
            token);

        return result.Success
            ? InteractionResult.Success
            : InteractionResult.Failed;
    }
}
```

## Как это ложится в registry

```text
PickupWorldObject
  ITargetable
  IInteractable
  IPickup
  IDisplayNameProvider
```

Тогда:

```text
Targeting выбирает ITargetable
Interaction дергает IInteractable
Inventory/GAS может напрямую работать с IPickup
```

## Почему нужен отдельный `IPickup`

Потому что pickup имеет свою доменную логику:

```text
какой ItemDefinition выдать
сколько штук
можно ли поднять
куда положить
исчезнуть ли из мира
проиграть ли VFX/SFX
```

А `IInteractable` — слишком общий. Дверь, NPC, сундук и предмет на земле все interactable, но не все pickup.

## Итог

Да, вводи отдельный интерфейс, но я бы выбрал имя:

```text
IPickup — если это именно предмет на земле
ICollectible — если это более общее: монетки, ресурсы, орбы, квестовые объекты
```

Для RPG-предметов на земле лучше **`IPickup`**.
