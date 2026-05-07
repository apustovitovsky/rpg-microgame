namespace Etheria.Features.StoryletSystem
{
    public sealed class WorldAttributePrecondition : IStoryletPrecondition
    {
        private readonly AttributeRequirement _requirement;
        private readonly string _description;

        public WorldAttributePrecondition(AttributeRequirement requirement, string description)
        {
            _requirement = requirement;
            _description = description;
        }

        public bool IsSatisfied(
            StoryletWorldState worldState,
            StoryletPlannerMemory memory,
            out StoryletRejectionReason rejectionReason)
        {
            if (!worldState.WorldAttributes.Matches(_requirement))
            {
                rejectionReason = new StoryletRejectionReason(
                    "missing_attribute_threshold",
                    _description);
                return false;
            }

            rejectionReason = null;
            return true;
        }
    }
}
