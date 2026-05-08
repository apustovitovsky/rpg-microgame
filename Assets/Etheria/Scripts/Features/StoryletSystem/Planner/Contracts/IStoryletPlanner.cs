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
}
