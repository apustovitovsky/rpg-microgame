using Etheria.Core.DI;
using Etheria.Game.DI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Etheria.Game
{
    [CreateAssetMenu(
    fileName = "LoadingScreenInstaller",
    menuName = "Etheria/Game/Loading Screen Installer")]
    public class LoadingScreenInstallerSO : InstallerSO
    {
        [SerializeField] private LoadingScreenSettingsSO _loadingScreenSettings;
        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterComponentInNewPrefab(
                _loadingScreenSettings.LoadingScreenView,
                Lifetime.Singleton)
                .UnderScopeRoot();

            builder.RegisterEntryPoint<LoadingScreenService>(Lifetime.Singleton)
                .As<ILoadingScreenService>();
        }
    }
}
