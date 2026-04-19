using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RPG.Core
{
    [CreateAssetMenu(fileName = "ProjectInstaller", menuName = "RPG/Core/Installers/Project Installer")]
    public class ProjectInstallerSO : InstallerSO
    {
        [SerializeField] private ProjectConfigSO _projectConfig;

        public override void Install(in InstallContext context)
        {
            var builder = context.Builder;

            builder.RegisterInstance(_projectConfig);

            builder.Register<SceneLoadingService>(Lifetime.Singleton)
                .As<ISceneLoadingService>();

            builder.RegisterEntryPoint<SceneCoordinator>()
                .As<ISceneCoordinator>();

            builder.Register<GameTimeService>(Lifetime.Singleton)
                .As<ITimeProvider>();

            builder.RegisterComponentInNewPrefab(_projectConfig.LoadingScreenView, Lifetime.Singleton)
                .DontDestroyOnLoad();

            builder.Register<LoadingScreenPresenter>(Lifetime.Singleton);

            builder.Register<SceneReadinessChannel>(Lifetime.Singleton);

            builder.RegisterBuildCallback(container =>
            {
                var scope = container.Resolve<LifetimeScope>();
                var view = container.Resolve<LoadingScreenView>();
                view.transform.SetParent(scope.transform, false);
            });
        }
    }
}