using Etheria.Core.DI;
using Etheria.Game.UI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Etheria.Features.Character
{
    [CreateAssetMenu(
        fileName = "CharacterUiInstaller",
        menuName = "Etheria/Features/Character/UI Installer")]
    public sealed class CharacterUiInstallerSO : InstallerSO
    {
        [SerializeField]
        private CharacterLabelView _labelPrefab;

        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<CharacterUiPoolHost>();

            builder.RegisterInstance(_labelPrefab);

            builder.Register<ICharacterLabelPoolRoots>(
                resolver => resolver.Resolve<CharacterUiPoolHost>().Labels,
                Lifetime.Singleton);

            builder.Register<CharacterLabelPool>(Lifetime.Singleton);

        }
    }
}
