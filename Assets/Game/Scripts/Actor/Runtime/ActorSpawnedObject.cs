using Game.Interaction;
using Game.Pickup;
using Game.Targeting;
using Game.World;

namespace Game.Actor
{
    public sealed class ActorSpawnedObject
    {
        public ActorSpawnedObject(
            WorldId worldId,
            IWorldActor actor,
            IActorView view,
            IDisplayable displayInfo,
            IInteractor interactor,
            IActorInputBinder inputBinder,
            ITargetProvider targetProvider,
            IInteractable interaction,
            IActorDialogue dialogue,
            IActorNavigation travel,
            IPickupEffectHandlerProvider pickupEffectHandlerProvider)
        {
            WorldId = worldId;
            Actor = actor;
            View = view;
            DisplayInfo = displayInfo;
            Interactor = interactor;
            InputBinder = inputBinder;
            TargetProvider = targetProvider;
            Interaction = interaction;
            Dialogue = dialogue;
            Travel = travel;
            PickupEffectHandlerProvider = pickupEffectHandlerProvider;
        }

        public WorldId WorldId { get; }

        public IWorldActor Actor { get; }

        public IActorView View { get; }

        public IDisplayable DisplayInfo { get; }

        public IInteractor Interactor { get; }

        public IActorInputBinder InputBinder { get; }

        public ITargetProvider TargetProvider { get; }

        public IInteractable Interaction { get; }

        public IActorDialogue Dialogue { get; }

        public IActorNavigation Travel { get; }

        public IPickupEffectHandlerProvider PickupEffectHandlerProvider { get; }
    }
}