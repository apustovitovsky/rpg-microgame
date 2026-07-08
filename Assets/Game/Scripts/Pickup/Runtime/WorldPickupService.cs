using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.World;

namespace Game.Pickup
{
    public sealed class WorldPickupService : IPickupService
    {
        private readonly IWorldRegistry<IWorldPickup> _pickups;
        private readonly IWorldRegistry<IPickupEffectHandlerProvider> _handlerProviders;
        private readonly IWorldManager _world;

        public WorldPickupService(
            IWorldRegistry<IWorldPickup> pickups,
            IWorldRegistry<IPickupEffectHandlerProvider> handlerProviders,
            IWorldManager world)
        {
            _pickups = pickups;
            _handlerProviders = handlerProviders;
            _world = world;
        }

        public async UniTask<PickupResult> CollectAsync(
            WorldId collectorId,
            WorldId pickupId,
            CancellationToken token)
        {
            if (!_handlerProviders.TryGet(
                    collectorId,
                    out var handlerProvider))
            {
                return PickupResult.HandlerProviderNotFound;
            }

            if (!_pickups.TryGet(
                    pickupId,
                    out var pickup))
            {
                return PickupResult.PickupNotFound;
            }

            if (!pickup.IsCollectable)
                return PickupResult.CannotBeCollected;

            if (pickup.Definition == null)
                return PickupResult.CannotBeCollected;

            var canApplyAny = false;

            foreach (var effect in pickup.Definition.Effects)
            {
                if (effect == null)
                    continue;

                if (!handlerProvider.TryGet(
                        effect.GetType(),
                        out var handler))
                {
                    continue;
                }

                if (!handler.CanApply(effect, pickup))
                    continue;

                canApplyAny = true;
                break;
            }

            if (!canApplyAny)
                return PickupResult.EffectCannotApply;

            try
            {
                foreach (var effect in pickup.Definition.Effects)
                {
                    token.ThrowIfCancellationRequested();

                    if (effect == null)
                        continue;

                    if (!handlerProvider.TryGet(
                            effect.GetType(),
                            out var handler))
                    {
                        continue;
                    }

                    if (!handler.CanApply(effect, pickup))
                        continue;

                    await handler.ApplyAsync(effect, pickup, token);
                }

                await pickup.SetCollectedAsync(token);

                _world.Despawn(pickupId);

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