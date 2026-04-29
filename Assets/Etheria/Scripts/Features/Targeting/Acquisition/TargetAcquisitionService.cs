using System;
using Etheria.Game.Targeting;

namespace Etheria.Features.Targeting
{
    public sealed class TargetAcquisitionService : ITargetAcquisitionService
    {
        private readonly ITargetCandidateSnapshotProvider _snapshotProvider;
        private readonly ITargetCandidateSelector _candidateSelector;

        public TargetAcquisitionService(
            ITargetCandidateSnapshotProvider snapshotProvider,
            ITargetCandidateSelector candidateSelector)
        {
            _snapshotProvider = snapshotProvider;
            _candidateSelector = candidateSelector;
        }

        public TargetAcquireResult Acquire(ITargetable currentTarget)
        {
            var snapshot = _snapshotProvider.Capture();
            if (!_candidateSelector.TrySelectBest(
                    snapshot.Candidates,
                    snapshot.Count,
                    currentTarget,
                    out var bestCandidate))
            {
                return TargetAcquireResult.None;
            }

            var status = ReferenceEquals(bestCandidate.Targetable, currentTarget)
                ? TargetAcquireStatus.CurrentTarget
                : TargetAcquireStatus.NewTarget;

            return new TargetAcquireResult(status, bestCandidate.Targetable);
        }
    }
}
