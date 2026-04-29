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

        public bool TryAcquire(ITargetable currentTarget, out ITargetable target)
        {
            target = null;

            var snapshot = _snapshotProvider.Capture();
            if (!_candidateSelector.TrySelectBest(
                    snapshot.Candidates,
                    snapshot.Count,
                    currentTarget,
                    out var bestCandidate))
            {
                return false;
            }

            target = bestCandidate.Targetable;
            return true;
        }

    }
}
