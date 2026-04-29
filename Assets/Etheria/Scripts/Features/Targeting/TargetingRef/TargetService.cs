using System;

namespace Etheria.Features.Targeting
{
    public sealed class TargetingService2 : ITargetingService, ITargetingEvents
    {
        private readonly ITargetCandidateProvider _candidateProvider;
        private readonly ITargetCandidateSelector _selector;
        private readonly TargetCandidate[] _candidates;

        public event Action<ITargetable> TargetChanged;

        public ITargetable CurrentTarget { get; private set; }

        public TargetingService2(
            ITargetCandidateProvider candidateProvider,
            ITargetCandidateSelector selector,
            TargetingSettingsSO settings)
        {
            _candidateProvider = candidateProvider;
            _selector = selector;
            _candidates = new TargetCandidate[settings.MaxTargetCandidates];
        }

        public bool TrySelectBestTarget(ITargetable ignoredTarget = null)
        {
            var count = _candidateProvider.GetCandidates(
                _candidates,
                ignoredTarget);

            if (!_selector.TrySelectBest(
                    _candidates,
                    count,
                    CurrentTarget,
                    out var bestCandidate))
            {
                ClearTarget();
                return false;
            }

            SetCurrentTarget(bestCandidate.Targetable);
            return true;
        }

        public void ClearTarget()
        {
            if (CurrentTarget == null)
                return;

            var previousTarget = CurrentTarget;

            CurrentTarget = null;

            TargetChanged?.Invoke(null);
        }

        private void SetCurrentTarget(ITargetable target)
        {
            if (ReferenceEquals(CurrentTarget, target))
                return;

            CurrentTarget = target;

            TargetChanged?.Invoke(target);
        }

        public bool TryAcquireFromView()
        {
            throw new NotImplementedException();
        }

        public bool TrySetTarget(ITargetable target)
        {
            throw new NotImplementedException();
        }

        public bool IsValid(ITargetable target)
        {
            throw new NotImplementedException();
        }

        public bool ValidateCurrentTarget()
        {
            throw new NotImplementedException();
        }
    }
}