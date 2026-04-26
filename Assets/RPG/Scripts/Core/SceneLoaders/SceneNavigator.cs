using System.Threading;
using Cysharp.Threading.Tasks;
using RPG.Core.VContainer;
using UnityEngine;
using VContainer.Unity;

namespace RPG.Core
{
    public sealed class SceneNavigator : ISessionNavigator, IAsyncStartable
    {
        private readonly SceneNavigationConfigSO _sceneNavigationConfig;
        private readonly LoadingScreenPresenter _loadingScreenPresenter;
        private readonly SceneStackLoader _sceneStackLoader;
        private readonly SceneScopeLoadingService _sceneScopeLoadingService;

        public SceneNavigator(
            SceneNavigationConfigSO sceneNavigationConfig,
            LoadingScreenPresenter loadingScreenPresenter,
            SceneScopeLoadingService sceneScopeLoadingService,
            SceneStackLoader sceneStackLoader)
        {
            _sceneNavigationConfig = sceneNavigationConfig;
            _loadingScreenPresenter = loadingScreenPresenter;
            _sceneStackLoader = sceneStackLoader;
            _sceneScopeLoadingService = sceneScopeLoadingService;
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
            if (_sceneNavigationConfig == null)
            {
                Debug.LogError($"{nameof(SceneNavigator)} requires a {nameof(SceneNavigationConfigSO)}.");
                return;
            }

            if (_sceneNavigationConfig.SceneCatalog != null &&
                _sceneNavigationConfig.SceneCatalog.TryGet("RPG", out var definitions) &&
                definitions != null &&
                definitions.Length > 0)
            {
                await _sceneScopeLoadingService.LoadSceneStackWithScopesAsync(definitions, cancellation);
                return;
            }

            if (_sceneNavigationConfig.StartupExperience != null)
            {
                await LoadScene(_sceneNavigationConfig.StartupExperience, showLoadingScreen: true);
                return;
            }

            Debug.LogError(
                $"{nameof(SceneNavigator)} could not find startup scene configuration. " +
                $"Assign either {nameof(SceneNavigationConfigSO.SceneCatalog)} or {nameof(SceneNavigationConfigSO.StartupExperience)}.");
        }
    }
}
