using System.Threading;
using Cysharp.Threading.Tasks;
using Game.World;

namespace Game.Pickup
{
    public interface IItemPickupService
    {
        UniTask<CollectResult> CollectAsync(
            WorldId collectorId,
            ICollectable collectable,
            CancellationToken token);
    }
}