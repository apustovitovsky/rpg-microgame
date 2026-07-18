using System;
using Game.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Gameplay
{
    [CreateAssetMenu(
        fileName = "GameplayModuleBuilder",
        menuName = "Game/Gameplay/Gameplay Module Builder")]
    public sealed class GameplayModuleBuilder :
        ModuleBuilder
    {
        [SerializeField] private ActorSpawnCatalog _actors;
        [SerializeField] private PickupSpawnCatalog _pickups;

        public override void Install(
            IContainerBuilder builder)
        {
            if (_actors == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(GameplayModuleBuilder)} requires assigned " +
                    $"{nameof(ActorSpawnCatalog)}.");
            }

            if (_pickups == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(GameplayModuleBuilder)} requires assigned " +
                    $"{nameof(PickupSpawnCatalog)}.");
            }

            builder.RegisterInstance(_actors);
            builder.RegisterInstance(_pickups);

            builder.Register<ActorRuntimeRegistry>(
                    Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<SpawnPointResolver>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.RegisterEntryPoint<GameplayManager>(
                Lifetime.Singleton);
        }
    }
}