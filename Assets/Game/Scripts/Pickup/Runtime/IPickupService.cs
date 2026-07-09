using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.World;


namespace Game.Pickup
{
    public interface IPickupService
    {
        bool TryGet(
            WorldId worldId,
            out IWorldPickup pickup);

        UniTask<PickupResult> CollectAsync(
            WorldId collectorId,
            WorldId pickupId,
            CancellationToken token);
    }

    public interface IPickupRegistrationService
    {
        IDisposable Register(IWorldPickup pickup);

        IDisposable RegisterEffectHandlerProvider(
            WorldId worldId,
            IPickupEffectHandlerProvider provider);
    }
}