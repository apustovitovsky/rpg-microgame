using System;

namespace Etheria.Features.StoryletSystem
{
    public sealed class StoryletBeamBranchTrace
    {
        public StoryletBeamBranchTrace(
            string branchId,
            float totalScore,
            int snapshotId,
            string reason,
            string storyletKey)
        {
            BranchId = branchId ?? throw new ArgumentNullException(nameof(branchId));
            TotalScore = totalScore;
            SnapshotId = snapshotId;
            Reason = reason ?? string.Empty;
            StoryletKey = storyletKey ?? string.Empty;
        }

        public string BranchId { get; }
        public float TotalScore { get; }
        public int SnapshotId { get; }
        public string Reason { get; }
        public string StoryletKey { get; }
    }
}
