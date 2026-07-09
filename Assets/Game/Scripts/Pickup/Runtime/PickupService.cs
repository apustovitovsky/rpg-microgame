using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.World;

namespace Game.Pickup
{
    public sealed class PickupService :
        IPickupService,
        IPickupRegistrationService
    {
        private readonly WorldIndex<IWorldPickup> _pickups = new();
        private readonly WorldIndex<IPickupEffectHandlerProvider> _handlerProviders = new();
        private readonly IWorldManager _world;

        public PickupService(IWorldManager world)
        {
            _world = world;
        }

        public IDisposable RegisterEffectHandlerProvider(
            WorldId worldId,
            IPickupEffectHandlerProvider provider)
        {
            return _handlerProviders.Register(
                worldId,
                provider);
        }

        public IDisposable Register(IWorldPickup pickup)
        {
            if (pickup == null)
                throw new ArgumentNullException(nameof(pickup));

            return _pickups.Register(
                pickup.WorldId,
                pickup);
        }

        public bool TryGet(
            WorldId worldId,
            out IWorldPickup pickup)
        {
            return _pickups.TryGet(
                worldId,
                out pickup);
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

            if (!TryGet(
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