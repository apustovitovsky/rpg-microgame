using System.Collections.Generic;

namespace Etheria.Features.StoryletSystem
{
    public interface IStoryletSalienceEvaluator
    {
        StoryletSalienceEvaluation Evaluate(
            StoryletDefinition definition,
            IReadOnlyList<RoleAssignment> assignment,
            StoryletPlannerMemory memory);
    }
}
