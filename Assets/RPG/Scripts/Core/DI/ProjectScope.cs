using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RPG.Core
{
    public class ProjectScope : LifetimeScope
    {
        [SerializeField] private ProjectConfigSO _projectConfig;

        protected override void Configure(IContainerBuilder builder)
        {
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

            builder.RegisterBuildCallback(container =>
            {
                var view = container.Resolve<LoadingScreenView>();
                view.transform.SetParent(transform);
            });
        }
    }
}
