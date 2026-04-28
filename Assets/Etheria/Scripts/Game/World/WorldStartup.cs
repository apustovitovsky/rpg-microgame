using System.Threading;
using Cysharp.Threading.Tasks;
using Etheria.Core.DI;
using Etheria.Game.Camera;
using VContainer.Unity;

namespace Etheria.Game.DI
{
    public sealed class WorldStartup : IAsyncStartable
    {
        private readonly IGameNavigator _gameNavigator;
        private readonly SceneCatalogSO _sceneCatalog;

        public WorldStartup(
            SceneCatalogSO sceneCatalog,
            IGameNavigator gameNavigator)
        {
            _sceneCatalog = sceneCatalog;
            _gameNavigator = gameNavigator;
        }

        public UniTask StartAsync(CancellationToken cancellation = default)
        {
            if (_sceneCatalog.TryGet("World", out var entry) && entry != null)
            {
                return _gameNavigator.LoadScene(entry, showLoading: true, cancellation);
            }

            return UniTask.CompletedTask;
        }
    }
}
