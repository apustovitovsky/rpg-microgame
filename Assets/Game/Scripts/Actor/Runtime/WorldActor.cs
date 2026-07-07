using Game.Interaction;
using Game.Pickup;
using Game.Targeting;
using Game.World;
using UnityEngine;

namespace Game.Actor
{
    public sealed class WorldActor : IWorldObject
    {
        public WorldActor(
            WorldId worldId,
            string displayName,
            IActorView view,
            IActorTravelEndpoint travel = null,
            ITargetProvider targetProvider = null,
            IActorInputBinder inputBinder = null,
            IActorDialogueEndpoint dialogue = null,
            IInteractable interaction = null,
            IPickupEffectHandlerProvider pickupEffectHandlerProvider = null)
        {
            WorldId = worldId;
            DisplayName = string.IsNullOrWhiteSpace(displayName)
                ? worldId.ToString()
                : displayName.Trim();

            View = view;
            Travel = travel;
            TargetProvider = targetProvider;
            InputBinder = inputBinder;
            Dialogue = dialogue;
            Interaction = interaction;
            PickupEffectHandlerProvider = pickupEffectHandlerProvider;
        }

        public WorldId WorldId { get; }

        public string DisplayName { get; }

        public Transform Root => View.Root;

        public IActorView View { get; }

        public IActorTravelEndpoint Travel { get; }

        public ITargetProvider TargetProvider { get; }

        public IActorInputBinder InputBinder { get; }

        public IActorDialogueEndpoint Dialogue { get; }

        public IInteractable Interaction { get; }

        public IPickupEffectHandlerProvider PickupEffectHandlerProvider { get; }

        public bool TryGet<TEndpoint>(out TEndpoint endpoint)
            where TEndpoint : class
        {
            endpoint = null;

            if (this is TEndpoint actor)
                endpoint = actor;
            else if (View is TEndpoint view)
                endpoint = view;
            else if (Travel is TEndpoint travel)
                endpoint = travel;
            else if (TargetProvider is TEndpoint targetProvider)
                endpoint = targetProvider;
            else if (InputBinder is TEndpoint inputBinder)
                endpoint = inputBinder;
            else if (Dialogue is TEndpoint dialogue)
                endpoint = dialogue;
            else if (Interaction is TEndpoint interaction)
                endpoint = interaction;
            else if (PickupEffectHandlerProvider is TEndpoint pickupEffectHandlerProvider)
                endpoint = pickupEffectHandlerProvider;

            return endpoint != null;
        }
    }
}