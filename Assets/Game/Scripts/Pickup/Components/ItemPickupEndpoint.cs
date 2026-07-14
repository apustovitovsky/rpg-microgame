using System;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core;
using Game.Inventory;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Pickup
{
    [DisallowMultipleComponent]
    public sealed class ItemPickupEndpoint :
        MonoBehaviour,
        ICollectable,
        IPrefabInstaller
    {
        private IInventoryService _inventories;
        private ItemPickupFragment _itemPickup;

        public Guid InstanceId { get; private set; }

        public PickupDefinition Definition { get; private set; }

        public bool IsCollected { get; private set; }

        public void Install(
            IContainerBuilder builder)
        {
            builder.RegisterComponent(this)
                .AsSelf()
                .As<ICollectable>();
        }

        [Inject]
        public void Construct(
            PickupInstance instance,
            IInventoryService inventories)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            if (!instance.TryGetFragment(
                    out ItemPickupFragment itemPickup) ||
                itemPickup.Item == null ||
                itemPickup.Count <= 0)
            {
                throw new InvalidOperationException(
                    $"{nameof(PickupDefinition)} " +
                    $"'{instance.Definition.DisplayName}' requires a valid " +
                    $"{nameof(ItemPickupFragment)}.");
            }

            InstanceId = instance.InstanceId;
            Definition = instance.Definition;
            _itemPickup = itemPickup;

            _inventories = inventories
                ?? throw new ArgumentNullException(nameof(inventories));
        }

        public bool CanCollect(Guid collectorInstanceId)
        {
            return collectorInstanceId != Guid.Empty &&
                   InstanceId != Guid.Empty &&
                   _itemPickup != null &&
                   !IsCollected &&
                   isActiveAndEnabled &&
                   gameObject.activeInHierarchy &&
                   _inventories.CanAdd(
                       collectorInstanceId,
                       _itemPickup.Item,
                       _itemPickup.Count);
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
                    _itemPickup.Item,
                    _itemPickup.Count))
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