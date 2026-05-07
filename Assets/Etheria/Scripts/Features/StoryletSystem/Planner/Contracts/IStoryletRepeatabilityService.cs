namespace Etheria.Features.StoryletSystem
{
    public interface IStoryletRepeatabilityService
    {
        bool IsBlocked(
            StoryletDefinition definition,
            StoryletWorldState worldState,
            StoryletPlannerMemory memory,
            out StoryletRejectionReason rejectionReason);

        StoryletPlannerMemory Advance(
            StoryletPlannerMemory memory,
            StoryletInstantiationCandidate candidate,
            StoryletWorldState nextWorldState);
    }
}
