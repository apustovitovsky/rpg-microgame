using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace RPG.Core
{
    public sealed class SceneCoordinator : ISceneCoordinator, IAsyncStartable
    {
        private readonly ISceneLoadingService _sceneLoadingService;
        private readonly LoadingScreenPresenter _loadingScreenPresenter;
        private readonly ProjectConfigSO _projectConfig;
        private readonly LifetimeScope _rootScope;

        public SceneCoordinator(
            ISceneLoadingService sceneLoadingService,
            LoadingScreenPresenter loadingScreenPresenter,
            ProjectConfigSO gameplayConfig,
            LifetimeScope rootScope)
        {
            _sceneLoadingService = sceneLoadingService;
            _loadingScreenPresenter = loadingScreenPresenter;
            _projectConfig = gameplayConfig;
            _rootScope = rootScope;
        }

        private async UniTask LoadSceneWithLoadingScreen(string sceneName)
        {
            _loadingScreenPresenter.ShowLoadingScreen();

            var sceneScope = await _sceneLoadingService.LoadSceneAsync(sceneName, _rootScope);

            try
            {
                var initiator = sceneScope != null ? sceneScope.Container.ResolveOrDefault<ISceneInitiator>() : null;

                if (initiator != null)
                {
                    await initiator.Ready;
                }
            }
            finally
            {
                _loadingScreenPresenter.HideLoadingScreen();
            }
        }

        public async UniTask LoadMainMenuScene()
        {
            await LoadSceneWithLoadingScreen(_projectConfig.MainMenuScene);
        }

        public async UniTask LoadRPGScene()
        {
            await LoadSceneWithLoadingScreen(_projectConfig.RPGScene);
        }

        public async UniTask LoadFPSScene()
        {
            await LoadSceneWithLoadingScreen(_projectConfig.FPSScene);
        }

        public async UniTask StartAsync(CancellationToken cancellation = default)
        {
            await LoadMainMenuScene();
        }
    }
}