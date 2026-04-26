using System;
using UnityEngine;

namespace RPG.Gameplay
{
    public sealed class TargetingService : ITargetingService
    {
        private readonly ITargetDetectionService _targetDetectionService;

        public IActorTargetable CurrentTarget { get; private set; }

        public event Action<IActorTargetable> TargetChanged;

        public TargetingService(ITargetDetectionService targetDetectionService)
        {
            _targetDetectionService = targetDetectionService;
        }

        public bool TryAcquireFromView()
        {
            var candidates = _targetDetectionService.GetCandidates();
            for (var i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                if (!TrySetTarget(candidate))
                    continue;

                return true;
            }
            Debug.Log("Failed to acquire target from view");
            return false;
        }

        public bool TrySetTarget(IActorTargetable target)
        {
            if (!IsValid(target))
                return false;

            if (ReferenceEquals(CurrentTarget, target))
                return true;

            CurrentTarget = target;
            Debug.Log($"Target acquired: {CurrentTarget.DisplayName}");
            TargetChanged?.Invoke(CurrentTarget);
            return true;
        }

        public bool IsValid(IActorTargetable target)
        {
            return target != null && target.IsTargetable;
        }

        public void ClearTarget()
        {
            if (CurrentTarget == null)
                return;

            Debug.Log($"Target cleared: {CurrentTarget.DisplayName}");
            CurrentTarget = null;
            TargetChanged?.Invoke(null);
        }
    }
}
