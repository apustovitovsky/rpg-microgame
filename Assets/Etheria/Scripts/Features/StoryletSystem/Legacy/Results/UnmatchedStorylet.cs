using System;

namespace Etheria.Features.StoryletSystem
{
    // Legacy unmatched storylet record from the pre-planner greedy matcher flow.
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
