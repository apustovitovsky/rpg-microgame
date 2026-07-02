namespace Game.Actor
{
    public sealed class ActorSpawnContext
    {
        public ActorSpawnContext(
            string actorId,
            string locationId = "",
            string anchorKey = "")
        {
            ActorId = actorId?.Trim() ?? string.Empty;
            LocationId = locationId?.Trim() ?? string.Empty;
            AnchorKey = anchorKey?.Trim() ?? string.Empty;
        }

        public string ActorId { get; }
        public string LocationId { get; }
        public string AnchorKey { get; }
    }
}