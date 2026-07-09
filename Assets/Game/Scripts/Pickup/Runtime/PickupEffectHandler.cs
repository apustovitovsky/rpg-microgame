using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.World;

namespace Game.Pickup
{
    public interface IPickupEffectHandler
    {
        Type EffectType { get; }

        bool CanApply(
            WorldId collectorId,
            PickupEffect effect,
            IPickup pickup);

        UniTask ApplyAsync(
            WorldId collectorId,
            PickupEffect effect,
            IPickup pickup,
            CancellationToken token);
    }

    public abstract class PickupEffectHandler<TEffect> : IPickupEffectHandler
        where TEffect : PickupEffect
    {
        public Type EffectType => typeof(TEffect);

        public bool CanApply(
            WorldId collectorId,
            PickupEffect effect,
            IPickup pickup)
        {
            return effect is TEffect typed &&
                   CanApply(collectorId, typed, pickup);
        }

        public UniTask ApplyAsync(
            WorldId collectorId,
            PickupEffect effect,
            IPickup pickup,
            CancellationToken token)
        {
            if (effect is not TEffect typed)
                return UniTask.CompletedTask;

            return ApplyAsync(
                collectorId,
                typed,
                pickup,
                token);
        }

        protected abstract bool CanApply(
            WorldId collectorId,
            TEffect effect,
            IPickup pickup);

        protected abstract UniTask ApplyAsync(
            WorldId collectorId,
            TEffect effect,
            IPickup pickup,
            CancellationToken token);
    }
}