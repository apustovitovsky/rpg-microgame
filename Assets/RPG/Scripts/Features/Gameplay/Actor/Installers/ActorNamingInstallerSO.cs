using RPG.Core;
using UnityEngine;
using VContainer;

namespace RPG.Gameplay
{
    [CreateAssetMenu(
        fileName = "ActorNamingInstaller",
        menuName = "RPG/Gameplay/Actor/Actor Naming Installer")]
    public sealed class ActorNamingInstallerSO : InstallerSO
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<ActorNamingService>(Lifetime.Singleton)
                .As<IActorNamingService>();
        }
    }
}