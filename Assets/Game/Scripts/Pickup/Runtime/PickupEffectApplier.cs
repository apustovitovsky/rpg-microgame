using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.World;

namespace Game.Pickup
{
    public sealed class PickupEffectApplier : IPickupEffectApplier
    {
        private readonly Dictionary<Type, IPickupEffectHandler> _handlers = new();

        public PickupEffectApplier(IEnumerable<IPickupEffectHandler> handlers)
        {
            foreach (var handler in handlers)
                Register(handler);
        }

        public bool CanApplyAny(
            WorldId collectorId,
            IPickup pickup)
        {
            if (pickup?.Definition == null)
                return false;

            foreach (var effect in pickup.Definition.Effects)
            {
                if (effect == null)
                    continue;

                if (!_handlers.TryGetValue(effect.GetType(), out var handler))
                    continue;

                if (handler.CanApply(collectorId, effect, pickup))
                    return true;
            }

            return false;
        }

        public async UniTask ApplyAllAsync(
            WorldId collectorId,
            IPickup pickup,
            CancellationToken token)
        {
            if (pickup?.Definition == null)
                return;

            foreach (var effect in pickup.Definition.Effects)
            {
                token.ThrowIfCancellationRequested();

                if (effect == null)
                    continue;

                if (!_handlers.TryGetValue(effect.GetType(), out var handler))
                    continue;

                if (!handler.CanApply(collectorId, effect, pickup))
                    continue;

                await handler.ApplyAsync(
                    collectorId,
                    effect,
                    pickup,
                    token);
            }
        }

        private void Register(IPickupEffectHandler handler)
        {
            if (handler == null)
                return;

            if (!_handlers.TryAdd(handler.EffectType, handler))
            {
                throw new InvalidOperationException(
                    $"Duplicate pickup effect handler for '{handler.EffectType.Name}'.");
            }
        }
    }
}