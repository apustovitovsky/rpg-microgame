using Game.Core;
using Game.Pickup;
using Game.Targeting;
using Game.World;
using UnityEngine;
using VContainer;

namespace Game.Gameplay
{
    [CreateAssetMenu(
        fileName = "PickupConfigurator",
        menuName = "Game/Gameplay/Pickup Configurator")]
    public sealed class PickupConfiguratorSO : BuildConfigurator
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<WorldRegistry<IWorldPickup>>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<WorldRegistry<ITargetable>>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<WorldPickupService>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<PickupWorldObjectFactory>(Lifetime.Singleton)
                .AsImplementedInterfaces();
        }
    }
}