using System.Threading;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace RPG.Core
{
    public sealed class SceneCoordinator : ISceneCoordinator, IAsyncStartable
    {
        private readonly ISceneLoadingService _sceneLoadingService;
        private readonly LoadingScreenPresenter _loadingScreenPresenter;
        private readonly ProjectConfigSO _projectConfig;
        private readonly LifetimeScope _rootScope;
        private readonly SceneReadinessChannel _readinessChannel;

        public SceneCoordinator(
            ISceneLoadingService sceneLoadingService,
            LoadingScreenPresenter loadingScreenPresenter,
            ProjectConfigSO projectConfig,
            LifetimeScope rootScope,
            SceneReadinessChannel readinessChannel)
        {
            _sceneLoadingService = sceneLoadingService;
            _loadingScreenPresenter = loadingScreenPresenter;
            _projectConfig = projectConfig;
            _rootScope = rootScope;
            _readinessChannel = readinessChannel;
        }

        private async UniTask LoadSceneWithLoadingScreen(string sceneName)
        {
            _readinessChannel.Reset();
            _loadingScreenPresenter.ShowLoadingScreen();

            try
            {
                await _sceneLoadingService.LoadSceneAsync(sceneName, _rootScope);
                await _readinessChannel.Completion;
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
