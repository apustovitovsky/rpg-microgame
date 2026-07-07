using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Pickup;
using UnityEngine;

namespace Game.Actor
{
    public sealed class DebugActorViewPickupEffectHandler :
        PickupEffectHandler<DebugActorViewPickupEffect>
    {
        private readonly IWorldActor _view;

        public DebugActorViewPickupEffectHandler(IWorldActor view)
        {
            _view = view;
        }

        protected override bool CanApply(
            DebugActorViewPickupEffect effect,
            IWorldPickup pickup)
        {
            return effect != null &&
                   pickup != null &&
                   _view != null;
        }

        protected override UniTask ApplyAsync(
            DebugActorViewPickupEffect effect,
            IWorldPickup pickup,
            CancellationToken token)
        {
            Debug.Log(
                $"{effect.Message}. Actor: '{_view.Root.name}'. Pickup: '{pickup.Definition?.name}'.");

            return UniTask.CompletedTask;
        }
    }
}