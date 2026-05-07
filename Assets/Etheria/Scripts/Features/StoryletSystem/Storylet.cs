using System;
using System.Collections.Generic;

namespace Etheria.Features.StoryletSystem
{
    public sealed class Storylet
    {
        public StoryletId Id { get; }
        public string Key { get; }
        public float Priority { get; }
        public IReadOnlyList<Role> Roles { get; }

        public Storylet(
            StoryletId id,
            string key,
            float priority,
            IReadOnlyList<Role> roles)
        {
            Id = id;
            Key = key;
            Priority = priority;
            Roles = roles ?? throw new ArgumentNullException(nameof(roles));
        }

        public override string ToString()
        {
            return Key;
        }
    }

    public sealed class StoryletMatchResult
    {
        private readonly List<MatchedStorylet> _matches = new();
        private readonly List<UnmatchedStorylet> _unmatchedStorylets = new();

        public IReadOnlyList<MatchedStorylet> Matches => _matches;
        public IReadOnlyList<UnmatchedStorylet> UnmatchedStorylets => _unmatchedStorylets;

        public void Add(Storylet storylet, IReadOnlyList<RoleAssignment> assignment)
        {
            _matches.Add(new MatchedStorylet(storylet, assignment));
        }

        public void AddUnmatched(Storylet storylet, string reason)
        {
            _unmatchedStorylets.Add(new UnmatchedStorylet(storylet, reason));
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

    public sealed class UnmatchedStorylet
    {
        public UnmatchedStorylet(Storylet storylet, string reason)
        {
            Storylet = storylet ?? throw new ArgumentNullException(nameof(storylet));
            Reason = reason ?? throw new ArgumentNullException(nameof(reason));
        }

        public Storylet Storylet { get; }
        public string Reason { get; }
    }
}
