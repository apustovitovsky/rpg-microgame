using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.World;

namespace Game.Pickup
{
    public sealed class PickupService : IPickupService
    {
        private readonly IPickupEffectApplier _effects;
        private readonly IWorldObjectRegistry _world;

        public PickupService(
            IPickupEffectApplier effects,
            IWorldObjectRegistry world)
        {
            _effects = effects;
            _world = world;
        }

        public async UniTask<PickupResult> CollectAsync(
            WorldId collectorId,
            IPickup pickup,
            CancellationToken token)
        {
            if (collectorId.IsEmpty)
                return PickupResult.InvalidCollector;

            if (pickup == null)
                return PickupResult.PickupNotFound;

            if (!pickup.IsCollectable)
                return PickupResult.CannotBeCollected;

            if (pickup.Definition == null)
                return PickupResult.CannotBeCollected;

            if (!_effects.CanApplyAny(collectorId, pickup))
                return PickupResult.EffectCannotApply;

            try
            {
                await _effects.ApplyAllAsync(
                    collectorId,
                    pickup,
                    token);

                await pickup.SetCollectedAsync(token);

                if (!pickup.WorldId.IsEmpty)
                    _world.Despawn(pickup.WorldId);

                return PickupResult.Succeeded;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                return PickupResult.Failed;
            }
        }
    }
}