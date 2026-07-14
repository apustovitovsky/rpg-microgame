using System;
using Game.Core;
using Game.Inventory;
using UnityEngine;
using VContainer;

namespace Game.Actor
{
    [DisallowMultipleComponent]
    public sealed class InventoryOwner :
        MonoBehaviour,
        IRegistryBindingSource<IInventory>
    {
        private Guid _instanceId;
        private IInventory _inventory;

        public Guid Id => _instanceId;

        public IInventory Value => _inventory;

        [Inject]
        public void Construct(ActorInstance instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            if (!instance.TryGetFragment(
                    out InventoryFragment fragment))
            {
                throw new InvalidOperationException(
                    $"{nameof(ActorDefinition)} for " +
                    $"'{instance.DisplayName}' requires " +
                    $"{nameof(InventoryFragment)}.");
            }

            _instanceId = instance.InstanceId;
            _inventory = fragment.Create();
        }
    }
}