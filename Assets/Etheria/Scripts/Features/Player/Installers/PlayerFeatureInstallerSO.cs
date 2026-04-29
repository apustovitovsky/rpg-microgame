using Etheria.Core.DI;
using UnityEngine;
using VContainer;


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

            builder.Register<ControlledActorProvider>(Lifetime.Singleton)
                .AsImplementedInterfaces();
        }
    }
}

