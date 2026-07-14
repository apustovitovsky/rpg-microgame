using Game.Core;
using Game.World;
using UnityEngine;
using VContainer;

namespace Game.Gameplay
{
    [CreateAssetMenu(
        fileName = "WorldModuleBuilder",
        menuName = "Game/Gameplay/World Module Builder")]
    public sealed class WorldModuleBuilder : ModuleBuilder
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<WorldSpawner>(Lifetime.Singleton)
                .AsImplementedInterfaces();
        }
    }
}