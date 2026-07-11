using Game.Core;
using Game.Loot;
using UnityEngine;
using VContainer;

namespace Game.Gameplay
{
    [CreateAssetMenu(
        fileName = "LootModuleBuilder",
        menuName = "Game/Gameplay/Loot Module Builder")]
    public sealed class LootModuleBuilder : ModuleBuilder
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<LootSessionService>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<LootContainerFactory>(Lifetime.Singleton);

            builder.Register<LootContainerSpawner>(Lifetime.Singleton)
                .AsImplementedInterfaces();
        }
    }
}