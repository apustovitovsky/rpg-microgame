using Etheria.Core.DI;
using Etheria.Game.Camera;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Etheria.Game.DI
{
    public class GameScope : LifetimeScope
    {
        [SerializeField] private GameSettingsSO _gameConfig;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<InputSystem_Actions>(Lifetime.Singleton);

            builder.RegisterComponentInNewPrefab(_gameConfig.LoadingScreenView, Lifetime.Singleton)
                .UnderTransform(transform);

            builder.RegisterEntryPoint<LoadingScreenService>(Lifetime.Singleton)
                .As<ILoadingScreenService>();

            builder.RegisterComponentInNewPrefab(_gameConfig.MainCamera, Lifetime.Singleton)
                .UnderTransform(transform)
                .As<IMainCameraProvider>();

            builder.Register<SceneStackLoadingService>(Lifetime.Singleton)
                .As<ISceneStackLoadingService>();

            builder.Register<GameNavigator>(Lifetime.Singleton)
                .As<IGameNavigator>();

            builder.RegisterEntryPoint<GameStartup>(Lifetime.Singleton)
                .WithParameter(_gameConfig.SceneCatalog);
        }
    }
}
