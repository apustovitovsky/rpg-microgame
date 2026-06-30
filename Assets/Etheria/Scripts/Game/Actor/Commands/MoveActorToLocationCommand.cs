using Etheria.Game.World;

namespace Etheria.Game.Commands
{
    public readonly struct MoveActorToLocationCommand : IActorCommand
    {
        public MoveActorToLocationCommand(
            string actorId,
            string locationId,
            string anchorKey,
            NavigationQueryFilter filter)
        {
            ActorId = actorId;
            LocationId = locationId;
            AnchorKey = anchorKey;
            Filter = filter;
        }

        public string ActorId { get; }

        public string LocationId { get; }

        public string AnchorKey { get; }

        public NavigationQueryFilter Filter { get; }
    }
}