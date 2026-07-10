using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Pickup
{
    public interface IItemPickupService
    {
        UniTask<CollectResult> CollectAsync(
            Guid collectorInstanceId,
            ICollectable collectable,
            CancellationToken token);
    }
}