namespace Etheria.Features.StoryletSystem
{
    public sealed class EntityTagPrecondition : IStoryletPrecondition
    {
        private readonly TagQuery _query;
        private readonly bool _mustExist;

        public EntityTagPrecondition(TagQuery query, bool mustExist, string description)
        {
            _query = query;
            _mustExist = mustExist;
            Description = description;
        }

        public string Description { get; }

        public bool IsSatisfied(
            StoryletWorldState worldState,
            StoryletPlannerMemory memory,
            out StoryletRejectionReason rejectionReason)
        {
            var exists = worldState.HasAnyEntity(_query);
            var isSatisfied = _mustExist ? exists : !exists;

            if (!isSatisfied)
            {
                rejectionReason = new StoryletRejectionReason(
                    "missing_entity_tag",
                    Description);
                return false;
            }

            rejectionReason = null;
            return true;
        }
    }
}
