namespace Etheria.Features.StoryletSystem
{
    public interface IStoryletScoringService
    {
        StoryletScoreBreakdown Evaluate(
            StoryletInstantiationCandidate candidate,
            StoryletWorldState currentWorldState,
            StoryletWorldState nextWorldState,
            StoryletPlannerMemory memory,
            int futureCandidateCount);
    }
}
