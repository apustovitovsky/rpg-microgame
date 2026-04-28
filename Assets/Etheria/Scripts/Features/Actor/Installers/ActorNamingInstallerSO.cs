using Etheria.Core.DI;
using Etheria.Game.Common;
using UnityEngine;
using VContainer;

namespace Etheria.Features.Actor
{
    [CreateAssetMenu(
        fileName = "ActorNamingInstaller",
        menuName = "Etheria/Gameplay/Actor/Actor Naming Installer")]
    public sealed class ActorNamingInstallerSO : ScopeInstallerSO
    {
        public override void Install(IContainerBuilder builder, GameObject rootObject)
        {
            builder.Register<ActorNameGenerator>(Lifetime.Singleton)
                .As<INameGenerator>();
        }
    }
}

