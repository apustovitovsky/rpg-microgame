Да, **`IPickupService` уместен**, но как orchestration-layer, не как владелец всех правил.

Лучше так:

```text
InteractionService
  -> понимает "interact with target"

PickupService
  -> понимает "collect pickup"
```

Flow:

```text
PickupService.Collect(collectorId, pickupId)
  -> registry.Get collector
  -> registry.Get pickup
  -> pickup.CanBePickedUp(context)
  -> collector.CanCollect(context)
  -> collector.Collect(context)
  -> pickup.MarkCollected(context)
```

Ответственность:

```text
IPickupService:
  собрать context
  вызвать проверки в правильном порядке
  выполнить collect transaction
  вернуть PickupResult

IPickup:
  состояние предмета

IPickupCollector:
  состояние сборщика / inventory / restrictions
```

То есть **да, сервис можно сделать**, особенно если pickup может запускаться не только через interaction:

```text
player interact
auto pickup trigger
quest script
loot all
magnet pickup
```

Тогда `PickupInteractable` просто вызывает:

```csharp
_pickupService.CollectAsync(context.InteractorId, context.TargetId, token);
```

Коротко: **`IPickupService` — хорошая идея, если pickup является отдельным доменным use-case.** Но проверки всё равно остаются в `IPickup` и `IPickupCollector`, а сервис только координирует.
