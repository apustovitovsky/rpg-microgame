using Etheria.Core.DI;

using UnityEngine;
using VContainer;


namespace Etheria.Features.Actor
{
    [CreateAssetMenu(
        fileName = "ActorFeatureInstaller",
        menuName = "Etheria/Features/Actor/Actor Feature Installer")]
    public class ActorFeatureInstallerSO : ScopeInstallerSO
    {
        [SerializeField]
        private ActorFeatureSettingsSO _featureSettings;

        public override void Install(IContainerBuilder builder, GameObject rootObject)
        {
            builder.RegisterInstance(_featureSettings);

            builder.Register<ActorFactory>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<ActorNameGenerator>(Lifetime.Singleton)
                .AsImplementedInterfaces();
        }
    }
}

