
using System.Threading;
using Cysharp.Threading.Tasks;

using VContainer.Unity;

namespace RPG.Core
{
    public sealed class SceneNavigator : ISessionNavigator, IAsyncStartable
    {
        private readonly LoadingScreenPresenter _loadingScreenPresenter;
        private readonly SceneStackLoader _sceneStackLoadingService;

        public SceneNavigator(
            LoadingScreenPresenter loadingScreenPresenter,
            SceneStackLoader sceneStackLoadingService)
        {
            _loadingScreenPresenter = loadingScreenPresenter;
            _sceneStackLoadingService = sceneStackLoadingService;
        }

        private async UniTask<bool> LoadScene(SceneStackLoadRequest request, bool showLoadingScreen = false)
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
            var request = new SceneStackLoadRequest("RPG/Scenes/MainMenuScene");
            await LoadScene(request, showLoadingScreen: true);
        }

        public async UniTask LoadRPGScene()
        {
            var request = new SceneStackLoadRequest(
                rootScenePath: "RPG/Scenes/GameplayScene",
                additiveScenes: new[]
                {
                    new SceneStackLoadRequest.AdditiveSceneRequest("RPG/Scenes/WorldScene", setActive: true)
                });

            await LoadScene(request, showLoadingScreen: true);
        }

        public async UniTask LoadFPSScene()
        {
            await LoadScene(new SceneStackLoadRequest("FPS/Scenes/MainScene"));
        }

        public async UniTask LoadSyntyScene()
        {
            await LoadScene(new SceneStackLoadRequest("Synty/AnimationBaseLocomotion/Samples/Scenes/Demo_01"));
        }

        public async UniTask StartAsync(CancellationToken cancellation = default)
        {
            await LoadRPGScene();
        }
    }
}
