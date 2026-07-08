using Game.Core;
using Game.Pickup;
using Game.Targeting;
using Game.World;
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
            builder.Register<WorldRegistry<IWorldPickup>>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<WorldRegistry<ITargetable>>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<WorldPickupService>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<PickupWorldRegistrar>(Lifetime.Singleton);

            builder.Register<PickupWorldObjectFactory>(Lifetime.Singleton);

            builder.Register<PickupSpawner>(Lifetime.Singleton)
                .AsImplementedInterfaces();
        }
    }
}