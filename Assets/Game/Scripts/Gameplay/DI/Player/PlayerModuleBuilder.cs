using Game.Core;
using Unity.Cinemachine;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Player
{
    [CreateAssetMenu(
        fileName = "PlayerModuleBuilder",
        menuName = "Game/Gameplay/Player Module Builder")]
    public sealed class PlayerModuleBuilder : ModuleBuilder
    {
        [SerializeField] private CinemachineCamera _virtualCameraPrefab;

        public override void Install(IContainerBuilder builder)
        {
            builder.Register<PlayerInputService>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<PlayerControlService>(Lifetime.Singleton)
                .AsSelf()
                .AsImplementedInterfaces();

            builder.RegisterEntryPoint<PlayerTargetNameplatePresenter>(
                Lifetime.Singleton);

            builder.RegisterComponentInNewPrefab(
                _virtualCameraPrefab,
                Lifetime.Singleton);

            builder.RegisterEntryPoint<PlayerActionController>(
                Lifetime.Singleton);
        }
    }
}