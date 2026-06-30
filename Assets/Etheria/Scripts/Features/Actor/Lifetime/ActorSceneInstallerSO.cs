using Etheria.Core.DI;
using UnityEngine;
using VContainer;


namespace Etheria.Actor
{
    [CreateAssetMenu(
        fileName = "ActorSceneInstaller",
        menuName = "Etheria/Actor/Actor Scene Installer")]
    public sealed class ActorSceneInstallerSO : InstallerSO
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<ActorActionGate>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<ActorCommandService>(Lifetime.Singleton)
                .AsImplementedInterfaces();
        }
    }
}