using Etheria.Core.DI;
using UnityEngine;
using VContainer;


namespace Etheria.Features.Actor
{
    [CreateAssetMenu(
        fileName = "ActorAnimationControllerInstaller",
        menuName = "Etheria/Features/Actor/ActorAnimationControllerInstaller")]
    public sealed class ActorAnimationControllerInstallerSO : ScopeInstallerSO
    {
        public override void Install(IContainerBuilder builder, GameObject rootObject)
        {
            builder.Register<SyntyActorAnimationController>(Lifetime.Singleton);
        }
    }
}
