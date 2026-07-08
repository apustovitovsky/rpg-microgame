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
    public sealed class GameplayConfiguration : BuildConfigurator
    {
        [SerializeField] private ActorSpawnCatalog _actors;
        [SerializeField] private PickupSpawnCatalog _pickups;

        public override void Install(IContainerBuilder builder)
        {
            if (_actors == null)
                throw new InvalidOperationException(
                    $"{nameof(GameplayConfiguration)} requires assigned {nameof(ActorSpawnCatalog)}.");

            if (_pickups == null)
                throw new InvalidOperationException(
                    $"{nameof(GameplayConfiguration)} requires assigned {nameof(PickupSpawnCatalog)}.");

            builder.RegisterInstance(_actors);
            builder.RegisterInstance(_pickups);

            builder.RegisterEntryPoint<GameplayManager>(
                Lifetime.Singleton);
        }
    }
}