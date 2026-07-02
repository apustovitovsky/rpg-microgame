using Game.CommandSystem;

namespace Game.Actor
{
    public readonly struct MoveToLocationCommand : ICommand
    {
        public MoveToLocationCommand(
            string actorId,
            string locationId,
            string anchorKey)
        {
            ActorId = actorId;
            LocationId = locationId;
            AnchorKey = anchorKey;
        }

        public string ActorId { get; }

        public string LocationId { get; }

        public string AnchorKey { get; }
    }
}