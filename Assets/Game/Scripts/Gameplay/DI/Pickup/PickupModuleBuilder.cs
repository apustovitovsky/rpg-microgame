using Game.Core;
using Game.Pickup;
using UnityEngine;
using VContainer;

namespace Game.Gameplay
{
    [CreateAssetMenu(
        fileName = "PickupModuleBuilder",
        menuName = "Game/Gameplay/Pickup ModuleBuilder")]
    public sealed class PickupModuleBuilder : ModuleBuilder
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<PickupService>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<PickupFactory>(Lifetime.Singleton);

            builder.Register<PickupSpawner>(Lifetime.Singleton)
                .AsImplementedInterfaces();
        }
    }
}