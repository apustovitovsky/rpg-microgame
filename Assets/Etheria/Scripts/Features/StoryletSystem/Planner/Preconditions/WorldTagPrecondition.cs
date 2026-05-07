namespace Etheria.Features.StoryletSystem
{
    public sealed class WorldTagPrecondition : IStoryletPrecondition
    {
        private readonly TagQuery _query;

        public WorldTagPrecondition(TagQuery query, string description)
        {
            _query = query;
            Description = description;
        }

        public string Description { get; }

        public bool IsSatisfied(
            StoryletWorldState worldState,
            StoryletPlannerMemory memory,
            out StoryletRejectionReason rejectionReason)
        {
            if (!_query.Matches(worldState.WorldTags))
            {
                rejectionReason = new StoryletRejectionReason("missing_world_tag", Description);
                return false;
            }

            rejectionReason = null;
            return true;
        }
    }
}
