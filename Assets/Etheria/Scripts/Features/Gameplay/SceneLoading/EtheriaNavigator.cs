using System.Threading;
using Cysharp.Threading.Tasks;
using Etheria.Core.DI;
using Etheria.Game;
using VContainer.Unity;

namespace Etheria.Features.Gameplay
{
    public sealed class EtheriaNavigator : IAsyncStartable
    {
        private readonly SceneCatalogSO _sceneCatalog;
        private readonly IGameNavigator _gameNavigator;

        public EtheriaNavigator(
            SceneCatalogSO sceneCatalog,
            IGameNavigator gameNavigator)
        {
            _sceneCatalog = sceneCatalog;
            _gameNavigator = gameNavigator;
        }

        public async UniTask StartAsync(CancellationToken ct = default)
        {
            if (_sceneCatalog.TryGet("Etheria", out var entry) && entry != null)
            {
                await _gameNavigator.LoadScene(entry, showLoading: true, ct);
            }
        }
    }
}


