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
        IRegistryBindingSource<InventoryInstance>
    {
        private Guid _instanceId;
        private InventoryInstance _inventory;

        public Guid Id => _instanceId;

        public InventoryInstance Value => _inventory;

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