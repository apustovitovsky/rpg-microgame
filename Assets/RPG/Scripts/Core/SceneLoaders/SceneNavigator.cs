
using System.Threading;
using Cysharp.Threading.Tasks;

using VContainer.Unity;

namespace RPG.Core
{
    public sealed class SceneNavigator : ISessionNavigator, IAsyncStartable
    {
        private readonly ProjectConfigSO _projectConfig;
        private readonly LoadingScreenPresenter _loadingScreenPresenter;
        private readonly SceneStackLoader _sceneStackLoadingService;

        public SceneNavigator(
            ProjectConfigSO projectConfig,
            LoadingScreenPresenter loadingScreenPresenter,
            SceneStackLoader sceneStackLoadingService)
        {
            _projectConfig = projectConfig;
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
            await LoadScene(_projectConfig.MainMenuSceneStack, showLoadingScreen: true);
        }

        public async UniTask LoadRPGScene()
        {
            await LoadScene(_projectConfig.RPGSceneStack, showLoadingScreen: true);
        }

        public async UniTask LoadFPSScene()
        {
            await LoadScene(_projectConfig.FPSSceneStack);
        }

        public async UniTask LoadSyntyScene()
        {
            await LoadScene(_projectConfig.SyntySceneStack);
        }

        public async UniTask StartAsync(CancellationToken cancellation = default)
        {
            await LoadScene(_projectConfig.StartupSceneStack);
        }
    }
}
