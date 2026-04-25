using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RPG.Core
{
    [CreateAssetMenu(fileName = "ProjectInstaller", menuName = "RPG/Core/Installers/Project Installer")]
    public class ProjectInstallerSO : InstallerSO
    {
        [SerializeField] private ProjectConfigSO _projectConfig;

        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_projectConfig);

            builder.Register<GameTimeService>(Lifetime.Singleton)
                .As<ITimeProvider>();

            builder.Register<InputSystem_Actions>(Lifetime.Singleton);

            builder.RegisterComponentInNewPrefab(_projectConfig.LoadingScreenView, Lifetime.Singleton)
                .DontDestroyOnLoad();

            builder.Register<LoadingScreenPresenter>(Lifetime.Singleton);

            builder.RegisterBuildCallback(container =>
            {
                var scope = container.Resolve<LifetimeScope>();
                var view = container.Resolve<LoadingScreenView>();
                view.transform.SetParent(scope.transform, false);
            });
        }
    }
}
