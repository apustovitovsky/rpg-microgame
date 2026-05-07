using System;
using System.Collections.Generic;

namespace Etheria.Features.StoryletSystem
{
    public sealed class StoryletBeamExpansionTrace
    {
        public StoryletBeamExpansionTrace(
            int depth,
            int beamWidth,
            IReadOnlyList<StoryletBeamBranchTrace> expandedBranches,
            IReadOnlyList<StoryletBeamBranchTrace> keptBranches,
            IReadOnlyList<StoryletBeamBranchTrace> prunedBranches)
        {
            Depth = depth;
            BeamWidth = beamWidth;
            ExpandedBranches = expandedBranches ?? throw new ArgumentNullException(nameof(expandedBranches));
            KeptBranches = keptBranches ?? throw new ArgumentNullException(nameof(keptBranches));
            PrunedBranches = prunedBranches ?? throw new ArgumentNullException(nameof(prunedBranches));
        }

        public int Depth { get; }
        public int BeamWidth { get; }
        public IReadOnlyList<StoryletBeamBranchTrace> ExpandedBranches { get; }
        public IReadOnlyList<StoryletBeamBranchTrace> KeptBranches { get; }
        public IReadOnlyList<StoryletBeamBranchTrace> PrunedBranches { get; }
    }
}
