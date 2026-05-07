using System;
using System.Collections.Generic;

namespace Etheria.Features.StoryletSystem
{
    // Legacy matched storylet record from the pre-planner greedy matcher flow.
    public sealed class MatchedStorylet
    {
        public MatchedStorylet(Storylet storylet, IReadOnlyList<RoleAssignment> assignment)
        {
            Storylet = storylet ?? throw new ArgumentNullException(nameof(storylet));
            Assignment = assignment ?? throw new ArgumentNullException(nameof(assignment));
        }

        public Storylet Storylet { get; }
        public IReadOnlyList<RoleAssignment> Assignment { get; }
    }
}
