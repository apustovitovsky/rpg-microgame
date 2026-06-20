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
    public sealed class CharacterUiInstallerSO : ScopeInstallerSO
    {
        [SerializeField]
        private CharacterLabelView _labelPrefab;

        public override void Install(
            IContainerBuilder builder,
            GameObject rootObject)
        {
            CharacterUiPoolHost host =
                rootObject.GetComponentInChildren<CharacterUiPoolHost>(true);

            builder.RegisterComponent(host);

            builder.RegisterInstance(_labelPrefab);

            builder.Register<ICharacterLabelPoolRoots>(
                _ => host.Labels,
                Lifetime.Singleton);

            builder.Register<CharacterLabelPool>(Lifetime.Singleton);

            builder.RegisterEntryPoint<CharacterLabelPresenter>(Lifetime.Singleton);
        }
    }
}