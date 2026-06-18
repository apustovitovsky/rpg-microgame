using System.Threading;
using Cysharp.Threading.Tasks;
using Etheria.Core.DI;

namespace Etheria.Game
{
    public class GameNavigator : IGameNavigator
    {
        private readonly ILoadingScreenService _loadingScreen;
        private readonly ISceneStackLoadingService _sceneScopeLoadingService;

        public GameNavigator(
            ILoadingScreenService loadingScreen,
            ISceneStackLoadingService sceneStackLoadingService
        )
        {
            _loadingScreen = loadingScreen;
            _sceneScopeLoadingService = sceneStackLoadingService;
        }

        public async UniTask<bool> LoadScene(
            SceneCatalogEntry entry,
            bool showLoadingScreen,
            CancellationToken ct = default)
        {
            if (showLoadingScreen)
                _loadingScreen.ShowLoading();

            try
            {
                var loadedScopes = await _sceneScopeLoadingService.LoadSceneStackAsync(entry, ct);
                return loadedScopes != null && loadedScopes.Count > 0;
            }
            finally
            {
                if (showLoadingScreen)
                    _loadingScreen.HideLoading();
            }
        }
    }
}
