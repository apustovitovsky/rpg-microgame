using System.Collections.Generic;

namespace Etheria.Features.StoryletSystem
{
    public interface IStoryletPlanner
    {
        StoryletPlannerResult Plan(
            StoryletWorldState initialWorldState,
            IReadOnlyList<StoryletDefinition> storylets,
            StoryletPlannerMemory initialMemory = null);
    }

    public interface IStoryletInstantiationService
    {
        StoryletInstantiationResult TryInstantiate(
            StoryletDefinition definition,
            StoryletWorldState worldState,
            StoryletPlannerMemory memory);
    }

    public interface IStoryletEffectApplier
    {
        StoryletWorldState Apply(
            StoryletInstantiationCandidate candidate,
            StoryletWorldState worldState);
    }

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

    public interface IStoryletSalienceEvaluator
    {
        StoryletSalienceEvaluation Evaluate(
            StoryletDefinition definition,
            IReadOnlyList<RoleAssignment> assignment,
            StoryletPlannerMemory memory);
    }

    public interface IStoryletScoringService
    {
        StoryletScoreBreakdown Evaluate(
            StoryletInstantiationCandidate candidate,
            StoryletWorldState currentWorldState,
            StoryletWorldState nextWorldState,
            StoryletPlannerMemory memory,
            int futureCandidateCount);
    }

    public interface IStoryletTelemetryFormatter
    {
        string Format(StoryletPlannerResult result);
    }
}
