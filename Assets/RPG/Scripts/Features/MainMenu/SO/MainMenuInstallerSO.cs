using RPG.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RPG.MainMenu
{
    [CreateAssetMenu(fileName = "MainMenuInstaller", menuName = "RPG/MainMenu/Installers/MainMenuInstaller")]
    public sealed class MainMenuInstallerSO : InstallerSO
    {
        [SerializeField] private MainMenuConfigSO _mainMenuConfig;

        public override void Install(in InstallContext context)
        {
            var builder = context.Builder;

            builder.RegisterInstance(_mainMenuConfig);

            builder.RegisterEntryPoint<MainMenuSessionStarter>(Lifetime.Scoped);

            builder.RegisterComponentInNewPrefab(_mainMenuConfig.MainMenuView, Lifetime.Scoped);
            builder.RegisterEntryPoint<MainMenuPresenter>(Lifetime.Scoped);
        }
    }
}