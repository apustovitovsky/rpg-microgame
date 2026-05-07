using System;
using System.Collections.Generic;

namespace Etheria.Features.StoryletSystem
{
    public sealed class StoryletCandidate
    {
        public StoryletCandidate(
            Storylet storylet,
            IReadOnlyList<RoleAssignment> assignment,
            float score)
        {
            Storylet = storylet ?? throw new ArgumentNullException(nameof(storylet));
            Assignment = assignment ?? throw new ArgumentNullException(nameof(assignment));
            Score = score;
        }

        public Storylet Storylet { get; }
        public IReadOnlyList<RoleAssignment> Assignment { get; }
        public float Score { get; }
    }
}