using System.Threading;
using Cysharp.Threading.Tasks;
using Game.World;

namespace Game.Pickup
{
    public interface IPickupService
    {
        UniTask<PickupResult> CollectAsync(
            WorldId collectorId,
            WorldId pickupId,
            CancellationToken token);
    }
}