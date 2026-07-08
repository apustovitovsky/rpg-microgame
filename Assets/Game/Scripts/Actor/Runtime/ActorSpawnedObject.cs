using Game.Interaction;
using Game.Pickup;
using Game.Targeting;
using Game.World;

namespace Game.Actor
{
    public sealed class ActorSpawnedObject
    {
        public ActorSpawnedObject(
            IWorldObject worldObject,
            IWorldActor actor,
            IActorAnchors anchors,
            IDisplayInfo displayInfo,
            IWorldSpatial spatial,
            IActorInputBinder inputBinder,
            ITargetProvider targetProvider,
            IInteractable interaction,
            IActorDialogueEndpoint dialogue,
            IActorTravelEndpoint travel,
            IPickupEffectHandlerProvider pickupEffectHandlerProvider)
        {
            WorldObject = worldObject;
            Actor = actor;
            Anchors = anchors;
            DisplayInfo = displayInfo;
            Spatial = spatial;
            InputBinder = inputBinder;
            TargetProvider = targetProvider;
            Interaction = interaction;
            Dialogue = dialogue;
            Travel = travel;
            PickupEffectHandlerProvider = pickupEffectHandlerProvider;
        }

        public WorldId WorldId => WorldObject.WorldId;

        public IWorldObject WorldObject { get; }

        public IWorldActor Actor { get; }

        public IActorAnchors Anchors { get; }

        public IDisplayInfo DisplayInfo { get; }

        public IWorldSpatial Spatial { get; }

        public IActorInputBinder InputBinder { get; }

        public ITargetProvider TargetProvider { get; }

        public IInteractable Interaction { get; }

        public IActorDialogueEndpoint Dialogue { get; }

        public IActorTravelEndpoint Travel { get; }

        public IPickupEffectHandlerProvider PickupEffectHandlerProvider { get; }
    }
}