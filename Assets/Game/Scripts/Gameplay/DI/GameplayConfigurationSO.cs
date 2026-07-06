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
    public sealed class GameplayConfigurationSO : BuildConfiguratorSO
    {
        [SerializeField] private GameplayActorConfigSO _config;
        public override void Install(IContainerBuilder builder)
        {
            if (_config == null)
                throw new InvalidOperationException(
                    $"{nameof(GameplayConfigurationSO)} requires assigned {nameof(GameplayActorConfigSO)}.");

            builder.RegisterInstance(_config);

            builder.RegisterEntryPoint<GameplayManager>(
                Lifetime.Singleton);
        }
    }
}