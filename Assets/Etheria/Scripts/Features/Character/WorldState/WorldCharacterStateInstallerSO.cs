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

        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_setup);

            builder.Register<CharacterWorldStateService>(
                    Lifetime.Singleton)
                .As<ICharacterWorldStateService>();
        }
    }
}