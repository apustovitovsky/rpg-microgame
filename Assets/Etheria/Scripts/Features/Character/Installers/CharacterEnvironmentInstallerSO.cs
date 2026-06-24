using Etheria.Core.DI;
using Etheria.Game.Character;
using UnityEngine;
using VContainer;
using VContainer.Unity;


namespace Etheria.Features.Character
{
    [CreateAssetMenu(
        fileName = "CharacterEnvironmentInstaller",
        menuName = "Etheria/Features/Character/Character Environment Installer")]
    public class CharacterEnvironmentInstallerSO : InstallerSO
    {
        [SerializeField]
        private SyntyLookSettingsSO _syntyLookSettings;

        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_syntyLookSettings);

            builder.Register<CharacterSpawner>(
                Lifetime.Singleton);

            builder.Register<CharacterTravelService>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<CharacterWorldPresenter>(Lifetime.Singleton)
                .AsImplementedInterfaces();

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

