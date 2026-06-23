using Etheria.Core.DI;

using Etheria.Game.Character;

using UnityEngine;
using VContainer;
using VContainer.Unity;
using Etheria.Features.Character;



namespace Etheria.Features.Character
{
    [CreateAssetMenu(
        fileName = "ActorFeatureInstaller",
        menuName = "Etheria/Features/Actor/Actor Feature Installer")]
    public class ActorFeatureInstallerSO : InstallerSO
    {
        [SerializeField]
        private ActorFeatureSettingsSO _featureSettings;

        [SerializeField]
        private SyntyLookSettingsSO _syntyLookSettings;

        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_featureSettings);
            builder.RegisterInstance(_syntyLookSettings);

            builder.RegisterEntryPoint<NpcSpawner>(
                Lifetime.Singleton);

            builder.Register<CharacterNameProvider>(Lifetime.Singleton)
                .As<ICharacterNameProvider>();

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

