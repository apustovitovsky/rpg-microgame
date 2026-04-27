using System.Threading;
using Cysharp.Threading.Tasks;
using RPG.Core.VContainer;

namespace RPG.Game
{
    public class GameNavigatorDefault : IGameNavigator
    {
        private readonly LoadingScreenPresenter _loadingScreenPresenter;
        private readonly SceneScopeLoadingService _sceneScopeLoadingService;

        public GameNavigatorDefault(
            LoadingScreenPresenter loadingScreenPresenter,
            SceneScopeLoadingService sceneScopeLoadingService
        )
        {
            _loadingScreenPresenter = loadingScreenPresenter;
            _sceneScopeLoadingService = sceneScopeLoadingService;
        }

        public async UniTask<bool> LoadScene(
            SceneCatalogEntry request,
            bool showLoading,
            CancellationToken ct = default)
        {
            if (showLoading)
                _loadingScreenPresenter.ShowLoadingScreen();

            try
            {
                var loadedScopes = await _sceneScopeLoadingService.LoadSceneStackWithScopesAsync(request, ct);
                return loadedScopes != null && loadedScopes.Count > 0;
            }
            finally
            {
                if (showLoading)
                    _loadingScreenPresenter.HideLoadingScreen();
            }
        }
    }
}
