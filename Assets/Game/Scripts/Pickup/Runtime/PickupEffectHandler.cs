using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Pickup
{
    public interface IPickupEffectHandler
    {
        Type EffectType { get; }

        bool CanApply(PickupEffect effect, IWorldPickup pickup);

        UniTask ApplyAsync(
            PickupEffect effect,
            IWorldPickup pickup,
            CancellationToken token);
    }

    public abstract class PickupEffectHandler<TEffect> : IPickupEffectHandler
        where TEffect : PickupEffect
    {
        public Type EffectType => typeof(TEffect);

        public bool CanApply(PickupEffect effect, IWorldPickup pickup)
        {
            return effect is TEffect typed &&
                   CanApply(typed, pickup);
        }

        public UniTask ApplyAsync(
            PickupEffect effect,
            IWorldPickup pickup,
            CancellationToken token)
        {
            if (effect is not TEffect typed)
                return UniTask.CompletedTask;

            return ApplyAsync(typed, pickup, token);
        }

        protected abstract bool CanApply(
            TEffect effect,
            IWorldPickup pickup);

        protected abstract UniTask ApplyAsync(
            TEffect effect,
            IWorldPickup pickup,
            CancellationToken token);
    }
}