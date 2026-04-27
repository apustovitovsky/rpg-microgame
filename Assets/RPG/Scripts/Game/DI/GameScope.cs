using RPG.Core;
using RPG.Core.VContainer;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RPG.Game
{
    public class GameScope : LifetimeScope
    {
        [SerializeField] private SceneCatalogSO _sceneCatalog;
        [SerializeField] private LoadingScreenView _loadingScreenView;
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_sceneCatalog);

            builder.Register<GameTimeService>(Lifetime.Singleton)
                .As<ITimeProvider>();

            builder.Register<InputSystem_Actions>(Lifetime.Singleton);

            builder.RegisterComponentInNewPrefab(_loadingScreenView, Lifetime.Singleton)
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