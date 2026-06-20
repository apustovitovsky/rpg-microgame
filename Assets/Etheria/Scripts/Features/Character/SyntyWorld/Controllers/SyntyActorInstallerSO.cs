using Etheria.Core.DI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Etheria.Features.Actor
{
    [CreateAssetMenu(
        fileName = "SyntyActorInstaller",
        menuName = "Etheria/Features/Synty/Synty Actor Installer")]
    public class SyntyActorInstallerSO : ScopeInstallerSO
    {


        public override void Install(IContainerBuilder builder, GameObject rootObject)
        {
            var syntyCameraController = rootObject.GetComponentInChildren<SyntyCameraController>(true);
            var syntyPlayerController = rootObject.GetComponentInChildren<SyntyActorAnimationController>(true);

            if (syntyCameraController != null)
            {
                builder.RegisterComponent(syntyCameraController)
                    .AsSelf();
            }

            if (syntyPlayerController != null)
            {
                builder.RegisterComponent(syntyPlayerController)
                    .AsSelf();
            }

            builder.RegisterBuildCallback(container =>
            {
                if (syntyCameraController != null)
                {
                    container.Inject(syntyCameraController);
                }

                if (syntyPlayerController != null)
                {
                    container.Inject(syntyPlayerController);
                }
            });
        }
    }
}
