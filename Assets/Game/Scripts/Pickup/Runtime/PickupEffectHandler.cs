using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Pickup
{
    public interface IPickupEffectHandler
    {
        Type EffectType { get; }

        bool CanApply(PickupEffect effect, IPickup pickup);

        UniTask ApplyAsync(
            PickupEffect effect,
            IPickup pickup,
            CancellationToken token);
    }

    public abstract class PickupEffectHandler<TEffect> : IPickupEffectHandler
        where TEffect : PickupEffect
    {
        public Type EffectType => typeof(TEffect);

        public bool CanApply(PickupEffect effect, IPickup pickup)
        {
            return effect is TEffect typed &&
                   CanApply(typed, pickup);
        }

        public UniTask ApplyAsync(
            PickupEffect effect,
            IPickup pickup,
            CancellationToken token)
        {
            if (effect is not TEffect typed)
                return UniTask.CompletedTask;

            return ApplyAsync(typed, pickup, token);
        }

        protected abstract bool CanApply(
            TEffect effect,
            IPickup pickup);

        protected abstract UniTask ApplyAsync(
            TEffect effect,
            IPickup pickup,
            CancellationToken token);
    }
}