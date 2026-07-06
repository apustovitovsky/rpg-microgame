using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Interaction;
using UnityEngine;

namespace Game.Pickup
{
    [DisallowMultipleComponent]
    public sealed class PickupInteractible :
        MonoBehaviour,
        IInteractable
    {
        [SerializeField] private PickupComponent _pickup;

        public bool CanInteract(InteractionContext context)
        {
            if (_pickup == null ||
                context.Interactor == null)
            {
                return false;
            }

            if (!context.Interactor.TryGet<IPickupCollector>(
                    out var collector))
            {
                return false;
            }

            var pickupContext = new PickupContext(_pickup);

            return collector.CanReceive(pickupContext) &&
                   _pickup.CanCollect(pickupContext);
        }

        public async UniTask InteractAsync(
            InteractionContext context,
            CancellationToken token)
        {
            if (_pickup == null ||
                context.Interactor == null)
            {
                return;
            }

            if (!context.Interactor.TryGet<IPickupCollector>(
                    out var collector))
            {
                return;
            }

            var pickupContext = new PickupContext(_pickup);

            if (!collector.CanReceive(pickupContext) ||
                !_pickup.CanCollect(pickupContext))
            {
                return;
            }

            await collector.ReceiveAsync(
                pickupContext,
                token);

            await _pickup.CollectAsync(
                pickupContext,
                token);
        }
    }
}