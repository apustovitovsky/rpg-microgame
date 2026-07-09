using System.Threading;
using Cysharp.Threading.Tasks;
using Game.World;

namespace Game.Pickup
{
    public interface IPickupEffectApplier
    {
        bool CanApplyAny(
            WorldId collectorId,
            IPickup pickup);

        UniTask ApplyAllAsync(
            WorldId collectorId,
            IPickup pickup,
            CancellationToken token);
    }
}