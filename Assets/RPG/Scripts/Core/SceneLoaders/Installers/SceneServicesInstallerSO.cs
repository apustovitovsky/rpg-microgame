
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RPG.Core
{
    [CreateAssetMenu(fileName = "SceneServicesInstaller", menuName = "RPG/Core/SceneServicesInstaller")]
    public class SceneServicesInstallerSO : InstallerSO
    {
        [SerializeField] private SceneNavigationConfigSO _sceneNavigationConfig;

        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_sceneNavigationConfig);

            builder.RegisterEntryPoint<SceneNavigator>()
                .As<ISessionNavigator>();

            builder.Register<SceneStackLoader>(Lifetime.Singleton);
        }
    }
}

