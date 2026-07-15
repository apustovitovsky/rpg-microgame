using System;
using Game.Core;
using Game.Inventory;
using UnityEngine;
using VContainer;

namespace Game.Actor
{
    [DisallowMultipleComponent]
    public sealed class InventoryEndpoint :
        MonoBehaviour,
        IPrefabInstaller
    {
        public void Install(
            IContainerBuilder builder)
        {
            builder.Register(
                resolver =>
                {
                    var fragments =
                        resolver.Resolve<IFragmentProvider>();

                    if (!fragments.TryGetFragment(
                            out InventoryFragment fragment))
                    {
                        throw new InvalidOperationException(
                            $"{nameof(ActorDefinition)} requires " +
                            $"{nameof(InventoryFragment)}.");
                    }

                    return fragment.Create();
                },
                Lifetime.Scoped);

            builder.RegisterBinding<InventoryInstance>();
        }
    }
}