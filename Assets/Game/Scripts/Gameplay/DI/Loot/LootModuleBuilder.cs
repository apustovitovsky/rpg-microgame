using System;
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
        [SerializeField]
        private LootContainerAssetCatalog _catalog;

        public override void Install(IContainerBuilder builder)
        {
            if (_catalog == null)
            {
                throw new InvalidOperationException(
                    "Loot container asset catalog is required.");
            }

            builder.RegisterInstance(_catalog)
                .AsImplementedInterfaces();

            builder.Register<LootSessionService>(Lifetime.Singleton)
                .AsImplementedInterfaces();
        }
    }
}