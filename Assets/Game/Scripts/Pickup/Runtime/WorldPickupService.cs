using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.World;

namespace Game.Pickup
{
    public sealed class WorldPickupService : IPickupService
    {
        private readonly IWorldObjectRegistry _worldObjects;
        private readonly IWorldObjectRegistryWriter _worldObjectWriter;

        public WorldPickupService(
            IWorldObjectRegistry worldObjects,
            IWorldObjectRegistryWriter worldObjectWriter)
        {
            _worldObjects = worldObjects;
            _worldObjectWriter = worldObjectWriter;
        }

        public async UniTask<PickupResult> CollectAsync(
            WorldId collectorId,
            WorldId pickupId,
            CancellationToken token)
        {
            if (!_worldObjects.TryGetEndpoint<IPickupEffectHandlerProvider>(
                    collectorId,
                    out var handlerProvider))
            {
                return PickupResult.HandlerProviderNotFound;
            }

            if (!_worldObjects.TryGetEndpoint<IPickup>(
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

                if (!handlerProvider.TryGetHandler(
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

                    if (!handlerProvider.TryGetHandler(
                            effect.GetType(),
                            out var handler))
                    {
                        continue;
                    }

                    if (!handler.CanApply(effect, pickup))
                        continue;

                    await handler.ApplyAsync(effect, pickup, token);
                }

                await pickup.MarkCollectedAsync(token);

                _worldObjectWriter.Unregister(pickupId);

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