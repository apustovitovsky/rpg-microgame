using System.Collections.Generic;

namespace Etheria.Features.StoryletSystem
{
    // Legacy result container from the pre-planner greedy matcher flow.
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
}
