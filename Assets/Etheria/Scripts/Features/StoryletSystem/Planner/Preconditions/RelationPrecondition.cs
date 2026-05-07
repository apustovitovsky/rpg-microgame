namespace Etheria.Features.StoryletSystem
{
    public sealed class RelationPrecondition : IStoryletPrecondition
    {
        private readonly EntityId _fromEntityId;
        private readonly EntityId _toEntityId;
        private readonly TagQuery _query;
        private readonly bool _mustExist;
        private readonly string _description;

        public RelationPrecondition(
            EntityId fromEntityId,
            EntityId toEntityId,
            TagQuery query,
            bool mustExist,
            string description)
        {
            _fromEntityId = fromEntityId;
            _toEntityId = toEntityId;
            _query = query;
            _mustExist = mustExist;
            _description = description;
        }

        public bool IsSatisfied(
            StoryletWorldState worldState,
            StoryletPlannerMemory memory,
            out StoryletRejectionReason rejectionReason)
        {
            var relationIndex = worldState.CreateRelationIndex();
            var matches = _query.Matches(relationIndex.GetRelationTags(_fromEntityId, _toEntityId));

            if (matches != _mustExist)
            {
                rejectionReason = new StoryletRejectionReason("missing_relation", _description);
                return false;
            }

            rejectionReason = null;
            return true;
        }
    }
}
