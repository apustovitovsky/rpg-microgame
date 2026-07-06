using Game.Core;
using Game.World;
using UnityEngine;
using VContainer;

namespace Game.Gameplay
{
    [CreateAssetMenu(
        fileName = "WorldConfigurator",
        menuName = "Game/World/World Configurator")]
    public sealed class WorldConfiguratorSO : BuildConfiguratorSO
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<WorldObjectRegistry>(Lifetime.Singleton)
                .AsImplementedInterfaces();
        }
    }
}