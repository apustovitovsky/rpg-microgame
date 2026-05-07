namespace Etheria.Features.StoryletSystem
{
    public interface IStoryletPrecondition
    {
        bool IsSatisfied(
            StoryletWorldState worldState,
            StoryletPlannerMemory memory,
            out StoryletRejectionReason rejectionReason);
    }
}
