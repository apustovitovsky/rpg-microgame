using System;
using System.Collections.Generic;

namespace Game.Pickup
{
    public interface IPickupEffectHandlerProvider
    {
        bool TryGet(
            Type effectType,
            out IPickupEffectHandler handler);
    }

    public sealed class PickupEffectHandlerProvider :
        IPickupEffectHandlerProvider
    {
        private readonly Dictionary<Type, IPickupEffectHandler> _handlers = new();

        public PickupEffectHandlerProvider(
            IEnumerable<IPickupEffectHandler> handlers)
        {
            foreach (var handler in handlers)
                Register(handler);
        }

        public bool TryGet(
            Type effectType,
            out IPickupEffectHandler handler)
        {
            if (effectType == null)
            {
                handler = null;
                return false;
            }

            return _handlers.TryGetValue(effectType, out handler);
        }

        private void Register(IPickupEffectHandler handler)
        {
            if (handler == null)
                return;

            Register(handler.EffectType, handler);
        }

        private void Register(
            Type effectType,
            IPickupEffectHandler handler)
        {
            if (effectType == null)
                return;

            if (!_handlers.TryAdd(effectType, handler))
            {
                throw new InvalidOperationException(
                    $"Duplicate pickup effect handler for '{effectType.Name}'.");
            }
        }
    }
}