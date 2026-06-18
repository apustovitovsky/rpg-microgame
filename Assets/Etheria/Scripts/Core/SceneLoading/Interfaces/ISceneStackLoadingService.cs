using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace Etheria.Core.DI
{

    public interface ISceneStackLoadingService
    {
        UniTask<IReadOnlyList<LifetimeScope>> LoadSceneStackAsync(
            SceneCatalogEntry entry,
            CancellationToken ct);
    }
}