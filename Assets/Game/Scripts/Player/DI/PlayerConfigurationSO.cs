using Game.Core;
using Unity.Cinemachine;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Player
{
    [CreateAssetMenu(
        fileName = "PlayerConfigurator",
        menuName = "Game/Player/Player Configurator")]
    public sealed class PlayerConfiguratorSO : BuildConfiguratorSO
    {
        [SerializeField] private CinemachineCamera _virtualCameraPrefab;

        public override void Install(IContainerBuilder builder)
        {
            builder.Register<PlayerInputService>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<PlayerService>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<PlayerActorSpawner>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.RegisterEntryPoint<PlayerTargetNameplatePresenter>(
                Lifetime.Singleton);

            builder.RegisterComponentInNewPrefab(
                _virtualCameraPrefab,
                Lifetime.Singleton);
        }
    }
}