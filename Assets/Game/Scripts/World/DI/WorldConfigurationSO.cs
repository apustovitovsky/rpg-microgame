using System;
using Game.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.World
{
    [CreateAssetMenu(
        fileName = "WorldConfiguration",
        menuName = "Game/World/World Configuration")]
    public sealed class WorldConfigurationSO : BuildConfiguratorSO
    {
        [SerializeField] private WorldActorConfigSO _config;
        public override void Install(IContainerBuilder builder)
        {
            if (_config == null)
                throw new InvalidOperationException(
                    $"{nameof(WorldConfigurationSO)} requires assigned {nameof(WorldActorConfigSO)}.");

            builder.RegisterInstance(_config);

            builder.RegisterEntryPoint<WorldActorLifecycleManager>(
                Lifetime.Singleton);
        }
    }
}