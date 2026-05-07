namespace Etheria.Features.StoryletSystem
{
    public sealed class EntityExistencePrecondition : IStoryletPrecondition
    {
        private readonly EntityId _entityId;
        private readonly bool _mustExist;
        private readonly string _description;

        public EntityExistencePrecondition(EntityId entityId, bool mustExist, string description)
        {
            _entityId = entityId;
            _mustExist = mustExist;
            _description = description;
        }

        public bool IsSatisfied(
            StoryletWorldState worldState,
            StoryletPlannerMemory memory,
            out StoryletRejectionReason rejectionReason)
        {
            var exists = worldState.ContainsEntity(_entityId);

            if (exists != _mustExist)
            {
                rejectionReason = new StoryletRejectionReason("entity_existence", _description);
                return false;
            }

            rejectionReason = null;
            return true;
        }
    }
}
