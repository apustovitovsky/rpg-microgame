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

        private async UniTask LoadScene(string sceneName, bool showLoadingScreen = true)
        {
            _readinessChannel.Reset();

            if (showLoadingScreen)
            {
                _loadingScreenPresenter.ShowLoadingScreen();
            }

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
            await LoadScene(_projectConfig.MainMenuScene, showLoadingScreen: false);
        }

        public async UniTask LoadRPGScene()
        {
            await LoadScene(_projectConfig.RPGScene);
        }

        public async UniTask LoadFPSScene()
        {
            await LoadScene(_projectConfig.FPSScene, showLoadingScreen: false);
        }

        public async UniTask LoadSyntyScene()
        {
            await LoadScene(_projectConfig.SyntyScene, showLoadingScreen: false);
        }

        public async UniTask StartAsync(CancellationToken cancellation = default)
        {
            await LoadRPGScene();
        }
    }
}
