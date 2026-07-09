using Game.Core;
using Game.World;
using UnityEngine;
using VContainer;

namespace Game.Gameplay
{
    [CreateAssetMenu(
        fileName = "WorldModuleBuilder",
        menuName = "Game/World/World Module Builder")]
    public sealed class WorldModuleBuilder : ModuleBuilder
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<WorldIdFactory>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<WorldManager>(Lifetime.Singleton)
                .AsImplementedInterfaces();
        }
    }
}