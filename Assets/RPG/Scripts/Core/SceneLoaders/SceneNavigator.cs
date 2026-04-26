
using System.Threading;
using Cysharp.Threading.Tasks;

using VContainer.Unity;

namespace RPG.Core
{
    public sealed class SceneNavigator : ISessionNavigator, IAsyncStartable
    {
        private readonly SceneNavigationConfigSO _sceneNavigationConfig;
        private readonly LoadingScreenPresenter _loadingScreenPresenter;
        private readonly SceneStackLoader _sceneStackLoader;

        public SceneNavigator(
            SceneNavigationConfigSO sceneNavigationConfig,
            LoadingScreenPresenter loadingScreenPresenter,
            SceneStackLoader sceneStackLoader)
        {
            _sceneNavigationConfig = sceneNavigationConfig;
            _loadingScreenPresenter = loadingScreenPresenter;
            _sceneStackLoader = sceneStackLoader;
        }

        private async UniTask<bool> LoadScene(ExperienceDefinitionSO request, bool showLoadingScreen = false)
        {
            if (showLoadingScreen)
                _loadingScreenPresenter.ShowLoadingScreen();

            try
            {
                return await _sceneStackLoader.LoadExperienceAsync(request, CancellationToken.None);
            }
            finally
            {
                if (showLoadingScreen)
                    _loadingScreenPresenter.HideLoadingScreen();
            }
        }

        public async UniTask LoadMainMenuScene()
        {
            await LoadScene(_sceneNavigationConfig.MainMenuExperience, showLoadingScreen: true);
        }

        public async UniTask LoadRPGScene()
        {
            await LoadScene(_sceneNavigationConfig.RpgExperience, showLoadingScreen: true);
        }

        public async UniTask LoadFPSScene()
        {
            await LoadScene(_sceneNavigationConfig.FpsExperience);
        }

        public async UniTask LoadSyntyScene()
        {
            await LoadScene(_sceneNavigationConfig.SyntyExperience);
        }

        public async UniTask StartAsync(CancellationToken cancellation = default)
        {
            await LoadScene(_sceneNavigationConfig.RpgExperience, showLoadingScreen: true);
        }
    }
}
