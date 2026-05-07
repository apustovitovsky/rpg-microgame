using System.Collections.Generic;

namespace Etheria.Features.StoryletSystem
{
    public interface IStoryletAssignmentBuilder
    {
        bool TryBuildAssignment(
            StoryletMatchingContext context,
            Storylet storylet,
            HashSet<Entity> freeEntities,
            out List<RoleAssignment> assignment);
    }
}
