using Etheria.Core.DI;
using UnityEngine;
using VContainer;

namespace Etheria.Gameplay
{
    [CreateAssetMenu(
        fileName = "ActorNamingInstaller",
        menuName = "Etheria/Gameplay/Actor/Actor Naming Installer")]
    public sealed class ActorNamingInstallerSO : ScopeInstallerSO
    {
        public override void Install(IContainerBuilder builder, GameObject rootObject)
        {
            builder.Register<ActorNamingService>(Lifetime.Singleton)
                .As<IActorNamingService>();
        }
    }
}
