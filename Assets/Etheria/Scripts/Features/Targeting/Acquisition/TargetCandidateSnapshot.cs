namespace Etheria.Features.Targeting
{
    public readonly struct TargetCandidateSnapshot
    {
        public TargetCandidateSnapshot(TargetCandidate[] candidates, int count)
        {
            Candidates = candidates;
            Count = count;
        }

        public TargetCandidate[] Candidates { get; }
        public int Count { get; }
    }
}
