using System.Threading;
using Cysharp.Threading.Tasks;
using Etheria.Core.DI;
using Etheria.Game.Camera;
using VContainer.Unity;

namespace Etheria.Game.DI
{
    public sealed class GameStartup : IAsyncStartable
    {
        private readonly IMainCameraProvider _cameraProvider;
        private readonly IGameNavigator _gameNavigator;
        private readonly SceneCatalogSO _sceneCatalog;

        public GameStartup(
            SceneCatalogSO sceneCatalog,
            IGameNavigator gameNavigator,
            IMainCameraProvider cameraProvider)
        {
            _sceneCatalog = sceneCatalog;
            _gameNavigator = gameNavigator;
            _cameraProvider = cameraProvider;
        }

        public UniTask StartAsync(CancellationToken cancellation = default)
        {
            if (_sceneCatalog.TryGet("FPS", out var entry) && entry != null)
            {
                return _gameNavigator.LoadScene(entry, showLoading: true, cancellation);
            }

            return UniTask.CompletedTask;
        }
    }
}
