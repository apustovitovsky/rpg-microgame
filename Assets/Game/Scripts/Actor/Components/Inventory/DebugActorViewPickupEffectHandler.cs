using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Pickup;
using UnityEngine;

namespace Game.Actor
{
    public sealed class DebugActorViewPickupEffectHandler :
        PickupEffectHandler<DebugActorViewPickupEffect>
    {
        private readonly IActorView _view;

        public DebugActorViewPickupEffectHandler(IActorView view)
        {
            _view = view;
        }

        protected override bool CanApply(
            DebugActorViewPickupEffect effect,
            IPickup pickup)
        {
            return effect != null &&
                   pickup != null &&
                   _view != null;
        }

        protected override UniTask ApplyAsync(
            DebugActorViewPickupEffect effect,
            IPickup pickup,
            CancellationToken token)
        {
            Debug.Log(
                $"{effect.Message}. Actor: '{_view.Root.name}'. Pickup: '{pickup.Definition?.name}'.");

            return UniTask.CompletedTask;
        }
    }
}