using Etheria.Core.DI;
using UnityEngine;
using VContainer;
using VContainer.Unity;


namespace Etheria.Features.Player
{
    [CreateAssetMenu(
        fileName = "PlayerControllerInstaller",
        menuName = "Etheria/Features/Player/Player Feature Installer")]
    public class PlayerFeatureInstallerSO : ScopeInstallerSO
    {
        public override void Install(IContainerBuilder builder, GameObject rootObject)
        {
            builder.Register<PlayerController>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<PlayerAvatarProvider>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.RegisterEntryPoint<PlayerAvatarFacingDriver>(Lifetime.Singleton);

            builder.Register<PlayerInputState>(Lifetime.Singleton);
        }
    }
}

