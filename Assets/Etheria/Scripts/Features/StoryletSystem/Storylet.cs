using System;
using System.Collections.Generic;

namespace Etheria.Features.StoryletSystem
{
    public sealed class Storylet
    {
        public string Id { get; }
        public float Priority { get; }
        public IReadOnlyList<Role> Roles { get; }

        public Storylet(
            string id,
            float priority,
            IReadOnlyList<Role> roles)
        {
            Id = id;
            Priority = priority;
            Roles = roles ?? throw new ArgumentNullException(nameof(roles));
        }
    }

    public sealed class StoryletMatchResult
    {
        private readonly List<MatchedStorylet> _matches = new();

        public IReadOnlyList<MatchedStorylet> Matches => _matches;

        public void Add(Storylet storylet, IReadOnlyList<RoleAssignment> assignment)
        {
            _matches.Add(new MatchedStorylet(storylet, assignment));
        }
    }

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
