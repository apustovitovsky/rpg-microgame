using System.Threading;
using Cysharp.Threading.Tasks;
using Etheria.Core.DI;

namespace Etheria.Game
{
    public interface IGameNavigator
    {
        UniTask<bool> LoadScene(
            SceneCatalogEntry request,
            bool showLoading,
            CancellationToken ct = default);
    }
}
