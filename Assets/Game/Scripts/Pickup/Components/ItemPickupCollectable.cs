using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Inventory;
using Game.World;
using UnityEngine;

namespace Game.Pickup
{
    [DisallowMultipleComponent]
    public sealed class ItemPickupCollectable :
        MonoBehaviour,
        ICollectable
    {
        private IInventoryService _inventories;

        public WorldId WorldId { get; private set; }

        public PickupDefinition Definition { get; private set; }

        public bool IsCollected { get; private set; }

        public void Initialize(
            WorldId worldId,
            PickupDefinition definition,
            IInventoryService inventories)
        {
            WorldId = worldId;
            Definition = definition;
            _inventories = inventories;
            IsCollected = false;
        }

        public bool CanCollect(WorldId collectorId)
        {
            return !collectorId.IsEmpty &&
                   !WorldId.IsEmpty &&
                   Definition != null &&
                   Definition.Item != null &&
                   Definition.Amount > 0 &&
                   _inventories != null &&
                   !IsCollected &&
                   isActiveAndEnabled &&
                   gameObject.activeInHierarchy &&
                   _inventories.CanAdd(
                       collectorId,
                       Definition.Item,
                       Definition.Amount);
        }

        public UniTask<CollectResult> CollectAsync(
            WorldId collectorId,
            CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (!CanCollect(collectorId))
                return UniTask.FromResult(CollectResult.CannotCollect);

            if (!_inventories.TryAdd(
                    collectorId,
                    Definition.Item,
                    Definition.Amount))
            {
                return UniTask.FromResult(CollectResult.CannotCollect);
            }

            IsCollected = true;

            LogInventory(collectorId);

            return UniTask.FromResult(CollectResult.Succeeded);
        }

        private void LogInventory(WorldId ownerId)
        {
            if (!_inventories.TryGet(ownerId, out var inventory))
                return;

            var log = new StringBuilder();

            log.Append("Inventory '")
                .Append(ownerId)
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