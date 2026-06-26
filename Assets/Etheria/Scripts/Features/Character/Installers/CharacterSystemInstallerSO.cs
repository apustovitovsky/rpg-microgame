using Etheria.Core.DI;
using Etheria.Game.Character;
using UnityEngine;
using VContainer;
using VContainer.Unity;


namespace Etheria.Features.Character
{
    [CreateAssetMenu(
        fileName = "CharacterSystemInstaller",
        menuName = "Etheria/Character/Character System Installer")]
    public class CharacterSystemInstallerSO : InstallerSO
    {
        [SerializeField]
        private SyntyLookSettingsSO _syntyLookSettings;

        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_syntyLookSettings);

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

