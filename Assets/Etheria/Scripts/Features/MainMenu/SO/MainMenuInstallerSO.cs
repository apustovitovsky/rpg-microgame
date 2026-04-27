using Etheria.Core.DI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Etheria.MainMenu
{
    [CreateAssetMenu(fileName = "MainMenuInstaller", menuName = "Etheria/MainMenu/Installers/MainMenuInstaller")]
    public sealed class MainMenuInstallerSO : InstallerSO
    {
        [SerializeField] private MainMenuConfigSO _mainMenuConfig;

        public override void Install(IContainerBuilder builder, GameObject rootObject)
        {
            builder.RegisterInstance(_mainMenuConfig);

            builder.RegisterEntryPoint<MainMenuSessionStarter>(Lifetime.Scoped);

            builder.RegisterComponentInNewPrefab(_mainMenuConfig.MainMenuView, Lifetime.Scoped);
            builder.RegisterEntryPoint<MainMenuPresenter>(Lifetime.Scoped);
        }
    }
}
