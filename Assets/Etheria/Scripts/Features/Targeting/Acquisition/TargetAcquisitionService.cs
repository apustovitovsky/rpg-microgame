using System;
using Etheria.Game.Targeting;

namespace Etheria.Features.Targeting
{
    public sealed class TargetAcquisitionService : ITargetAcquisitionService
    {
        private readonly ITargetCandidateSnapshotProvider _snapshotProvider;
        private readonly ITargetCandidateSelector _candidateSelector;
        private readonly ITargetCandidateValidityFilter _validityFilter;


        public TargetAcquisitionService(
            ITargetCandidateSnapshotProvider snapshotProvider,
            ITargetCandidateSelector candidateSelector,
            ITargetCandidateValidityFilter validityFilter)
        {
            _snapshotProvider = snapshotProvider;
            _candidateSelector = candidateSelector;
            _validityFilter = validityFilter;
        }



        public TargetAcquireResult Acquire(ITargetable currentTarget)
        {
            var snapshot = _snapshotProvider.Capture();
            var candidates = snapshot.Candidates;
            var count = snapshot.Count;

            var validCount = _validityFilter.FilterInPlace(candidates, count);

            if (validCount <= 0)
                return TargetAcquireResult.None;

            if (!_candidateSelector.TrySelectBest(
                    candidates,
                    validCount,
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
