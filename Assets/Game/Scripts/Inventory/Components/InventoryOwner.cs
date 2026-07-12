using System;
using Game.World;
using UnityEngine;
using VContainer;

namespace Game.Inventory
{
    [DisallowMultipleComponent]
    public sealed class InventoryOwner :
        MonoBehaviour,
        IInventoryOwner
    {
        private IWorldInstance _instance;

        public Guid InstanceId =>
            _instance != null
                ? _instance.InstanceId
                : Guid.Empty;

        public IInventory Inventory { get; private set; }

        [Inject]
        public void Construct(
            IWorldInstance instance,
            IInventory inventory)
        {
            _instance = instance
                ?? throw new ArgumentNullException(nameof(instance));

            Inventory = inventory
                ?? throw new ArgumentNullException(nameof(inventory));
        }
    }
}