using System.Threading;
using Cysharp.Threading.Tasks;
using RPG.Core.VContainer;
using RPG.Game;
using VContainer.Unity;

public sealed class RPGNavigator : IAsyncStartable
{
    private readonly SceneCatalogSO _sceneCatalog;
    private readonly IGameNavigator _gameNavigator;

    public RPGNavigator(
        SceneCatalogSO sceneCatalog,
        IGameNavigator gameNavigator)
    {
        _sceneCatalog = sceneCatalog;
        _gameNavigator = gameNavigator;
    }

    public async UniTask StartAsync(CancellationToken ct = default)
    {
        if (_sceneCatalog.TryGet("RPG", out var entry) && entry != null)
        {
            await _gameNavigator.LoadScene(entry, showLoadingScreen: true, ct);
        }
    }
}

