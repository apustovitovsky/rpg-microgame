using System;
using Game.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Gameplay
{
    [CreateAssetMenu(
        fileName = "GameplayConfiguration",
        menuName = "Game/Gameplay/Gameplay Configuration")]
    public sealed class GameplayConfiguration : ModuleBuilder
    {
        [SerializeField] private ActorSpawnCatalog _actors;

        public override void Install(IContainerBuilder builder)
        {
            if (_actors == null)
                throw new InvalidOperationException(
                    $"{nameof(GameplayConfiguration)} requires assigned {nameof(ActorSpawnCatalog)}.");

            builder.RegisterInstance(_actors);

            builder.Register<SpawnPointResolver>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.RegisterEntryPoint<GameplayManager>(
                Lifetime.Singleton);
        }
    }
}