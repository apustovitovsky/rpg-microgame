using System;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Inventory;
using UnityEngine;
using VContainer;

namespace Game.Pickup
{
    [DisallowMultipleComponent]
    public sealed class ItemPickupCollectable :
        MonoBehaviour,
        ICollectable
    {
        private IInventoryService _inventories;

        public Guid InstanceId { get; private set; }

        public PickupDefinition Definition { get; private set; }

        public bool IsCollected { get; private set; }

        [Inject]
        public void Construct(
            PickupInstance instance,
            IInventoryService inventories)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            Initialize(
                instance.InstanceId,
                instance.Definition,
                inventories);
        }

        public void Initialize(
            Guid instanceId,
            PickupDefinition definition,
            IInventoryService inventories)
        {
            if (instanceId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Pickup instance id is required.",
                    nameof(instanceId));
            }

            InstanceId = instanceId;

            Definition = definition != null
                ? definition
                : throw new ArgumentNullException(nameof(definition));

            _inventories = inventories
                ?? throw new ArgumentNullException(nameof(inventories));

            IsCollected = false;
        }

        public bool CanCollect(Guid collectorInstanceId)
        {
            return collectorInstanceId != Guid.Empty &&
                   InstanceId != Guid.Empty &&
                   Definition.Item != null &&
                   Definition.Amount > 0 &&
                   !IsCollected &&
                   isActiveAndEnabled &&
                   gameObject.activeInHierarchy &&
                   _inventories.CanAdd(
                       collectorInstanceId,
                       Definition.Item,
                       Definition.Amount);
        }

        public UniTask<CollectResult> CollectAsync(
            Guid collectorInstanceId,
            CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (!CanCollect(collectorInstanceId))
            {
                return UniTask.FromResult(
                    CollectResult.CannotCollect);
            }

            if (!_inventories.TryAdd(
                    collectorInstanceId,
                    Definition.Item,
                    Definition.Amount))
            {
                return UniTask.FromResult(
                    CollectResult.CannotCollect);
            }

            IsCollected = true;

            LogInventory(collectorInstanceId);

            return UniTask.FromResult(
                CollectResult.Succeeded);
        }

        private void LogInventory(Guid ownerInstanceId)
        {
            if (!_inventories.TryGet(
                    ownerInstanceId,
                    out var inventory))
            {
                return;
            }

            var log = new StringBuilder();

            log.Append("Inventory '")
                .Append(ownerInstanceId)
                .Append("' (")
                .Append(inventory.Entries.Count)
                .Append('/')
                .Append(inventory.Capacity)
                .AppendLine(" slots)");

            for (var index = 0;
                 index < inventory.Entries.Count;
                 index++)
            {
                var entry = inventory.Entries[index];

                log.Append("  [")
                    .Append(index)
                    .Append("] ")
                    .Append(entry.Definition.DisplayName)
                    .Append(" x")
                    .Append(entry.Count)
                    .Append(" | ")
                    .Append(entry.Instance.InstanceId)
                    .AppendLine();
            }

            Debug.Log(log.ToString(), this);
        }
    }
}