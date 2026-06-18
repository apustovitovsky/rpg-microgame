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
    public class LoadingScreenInstallerSO : ScopeInstallerSO
    {
        [SerializeField] private LoadingScreenSettingsSO _loadingScreenSettings;
        public override void Install(IContainerBuilder builder, GameObject rootObject)
        {
            builder.RegisterComponentInNewPrefab(
                _loadingScreenSettings.LoadingScreenView,
                Lifetime.Singleton)
            .UnderTransform(rootObject.transform);

            builder.RegisterEntryPoint<LoadingScreenService>(Lifetime.Singleton)
                .As<ILoadingScreenService>();
        }
    }
}