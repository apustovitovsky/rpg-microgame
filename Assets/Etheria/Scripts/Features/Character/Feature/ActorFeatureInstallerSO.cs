using Etheria.Core.DI;

using Etheria.Game.Character;

using UnityEngine;
using VContainer;
using VContainer.Unity;
using Etheria.Features.Character;



namespace Etheria.Features.Actor
{
    [CreateAssetMenu(
        fileName = "ActorFeatureInstaller",
        menuName = "Etheria/Features/Actor/Actor Feature Installer")]
    public class ActorFeatureInstallerSO : ScopeInstallerSO
    {
        [SerializeField]
        private ActorFeatureSettingsSO _featureSettings;

        [SerializeField]
        private SyntyLookSettingsSO _syntyLookSettings;

        public override void Install(IContainerBuilder builder, GameObject rootObject)
        {
            builder.RegisterInstance(_featureSettings);
            builder.RegisterInstance(_syntyLookSettings);

            builder.Register<ActorFactory>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<ActorNameGenerator>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<PlayerCharacterProvider>(Lifetime.Singleton)
                .As<IPlayerCharacterProvider>();

            builder.RegisterEntryPoint<SyntyWorldEntryPoint>(
                Lifetime.Singleton);
        }
    }
}

