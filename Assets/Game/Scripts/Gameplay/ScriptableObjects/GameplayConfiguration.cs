using System;
using Game.Core;
using Game.Loot;
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
        [SerializeField] private PickupSpawnCatalog _pickups;

        [SerializeField]
        private LootContainerSpawnCatalog _lootContainers;

        public override void Install(IContainerBuilder builder)
        {
            if (_actors == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(GameplayConfiguration)} requires assigned " +
                    $"{nameof(ActorSpawnCatalog)}.");
            }

            if (_pickups == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(GameplayConfiguration)} requires assigned " +
                    $"{nameof(PickupSpawnCatalog)}.");
            }

            if (_lootContainers == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(GameplayConfiguration)} requires assigned " +
                    $"{nameof(LootContainerSpawnCatalog)}.");
            }

            builder.RegisterInstance(_actors);
            builder.RegisterInstance(_pickups);
            builder.RegisterInstance(_lootContainers);

            builder.Register<SpawnPointResolver>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.RegisterEntryPoint<GameplayManager>(
                Lifetime.Singleton);
        }
    }
}