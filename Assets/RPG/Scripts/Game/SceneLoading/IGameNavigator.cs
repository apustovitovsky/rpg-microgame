using System.Threading;
using Cysharp.Threading.Tasks;
using RPG.Core.VContainer;

namespace RPG.Game
{
    public interface IGameNavigator
    {
        UniTask<bool> LoadScene(
            SceneCatalogEntry request,
            bool showLoadingScreen,
            CancellationToken ct = default);
    }
}