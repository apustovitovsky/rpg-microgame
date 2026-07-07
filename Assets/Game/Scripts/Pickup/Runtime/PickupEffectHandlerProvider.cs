using System;
using System.Collections.Generic;

namespace Game.Pickup
{
    public interface IPickupEffectHandlerProvider
    {
        bool TryGetHandler(
            Type effectType,
            out IPickupEffectHandler handler);
    }

    public sealed class PickupEffectHandlerProvider :
        IPickupEffectHandlerProvider
    {
        private readonly Dictionary<Type, IPickupEffectHandler> _handlers;

        public PickupEffectHandlerProvider(
            IEnumerable<IPickupEffectHandler> handlers)
        {
            _handlers = new Dictionary<Type, IPickupEffectHandler>();

            foreach (var handler in handlers)
            {
                if (handler == null)
                    continue;

                if (!_handlers.TryAdd(handler.EffectType, handler))
                {
                    throw new InvalidOperationException(
                        $"Duplicate pickup effect handler for '{handler.EffectType.Name}'.");
                }
            }
        }

        public bool TryGetHandler(
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
    }
}