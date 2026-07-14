using System;
using Game.Core;
using Game.Inventory;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Actor
{
    [DisallowMultipleComponent]
    public sealed class InventoryEndpoint :
        MonoBehaviour,
        IRegistryBindingSource<InventoryInstance>,
        IPrefabInstaller
    {
        private Guid _instanceId;
        private InventoryInstance _inventory;

        public Guid Id => _instanceId;

        public InventoryInstance Value => _inventory;

        public void Install(
            IContainerBuilder builder)
        {
            builder.RegisterComponent(this)
                .AsSelf()
                .As<IRegistryBindingSource<InventoryInstance>>();

            builder.RegisterEntryPoint<
                RegistryBinding<InventoryInstance>>(
                Lifetime.Scoped);
        }

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