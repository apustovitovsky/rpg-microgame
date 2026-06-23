using Etheria.Core.DI;
using Etheria.Game.Character;
using UnityEngine;
using VContainer;

namespace Etheria.Features.Character
{
    [CreateAssetMenu(
        fileName = "WorldCharacterStateInstaller",
        menuName = "Etheria/World/Character State Installer")]
    public sealed class WorldCharacterStateInstallerSO : InstallerSO
    {
        [SerializeField]
        private WorldCharacterSetupSO _setup;

        [SerializeField]
        private CharacterCatalogSO _catalog;

        public override void Install(IContainerBuilder builder)
        {
            _catalog.Validate();

            builder.RegisterInstance(_setup);
            builder.RegisterInstance(_catalog);

            builder.Register<CharacterWorldStateService>(
                    Lifetime.Singleton)
                .As<ICharacterWorldStateService>();
        }
    }
}
