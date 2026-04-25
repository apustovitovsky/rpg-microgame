
using System.Threading;
using Cysharp.Threading.Tasks;

using VContainer.Unity;

namespace RPG.Core
{
    public sealed class SceneNavigator : ISessionNavigator, IAsyncStartable
    {
        private readonly SceneNavigationConfigSO _sceneNavigationConfig;
        private readonly LoadingScreenPresenter _loadingScreenPresenter;
        private readonly SceneStackLoader _sceneStackLoadingService;

        public SceneNavigator(
            SceneNavigationConfigSO sceneNavigationConfig,
            LoadingScreenPresenter loadingScreenPresenter,
            SceneStackLoader sceneStackLoadingService)
        {
            _sceneNavigationConfig = sceneNavigationConfig;
            _loadingScreenPresenter = loadingScreenPresenter;
            _sceneStackLoadingService = sceneStackLoadingService;
        }

        private async UniTask<bool> LoadScene(SceneStackSO request, bool showLoadingScreen = false)
        {
            if (showLoadingScreen)
                _loadingScreenPresenter.ShowLoadingScreen();

            try
            {
                return await _sceneStackLoadingService.LoadStackAsync(request, CancellationToken.None);
            }
            finally
            {
                if (showLoadingScreen)
                    _loadingScreenPresenter.HideLoadingScreen();
            }
        }

        public async UniTask LoadMainMenuScene()
        {
            await LoadScene(_sceneNavigationConfig.MainMenuSceneStack, showLoadingScreen: true);
        }

        public async UniTask LoadRPGScene()
        {
            await LoadScene(_sceneNavigationConfig.RPGSceneStack, showLoadingScreen: true);
        }

        public async UniTask LoadFPSScene()
        {
            await LoadScene(_sceneNavigationConfig.FPSSceneStack);
        }

        public async UniTask LoadSyntyScene()
        {
            await LoadScene(_sceneNavigationConfig.SyntySceneStack);
        }

        public async UniTask StartAsync(CancellationToken cancellation = default)
        {
            await LoadScene(_sceneNavigationConfig.StartupSceneStack);
        }
    }
}
