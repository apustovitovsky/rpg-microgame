using System;
using System.Collections.Generic;
using Etheria.Game.Camera;
using Etheria.Game.Targeting;
using UnityEngine;

namespace Etheria.Features.Targeting
{
    public sealed class TargetingService : ITargetingService
    {
        private readonly ITargetCandidateProvider _candidateProvider;
        private readonly ITargetCandidateSelector _candidateSelector;
        private readonly ICameraTransformProvider _cameraProvider;
        private readonly TargetingSettingsSO _targetingSettings;
        private readonly ITargetLineOfSightChecker _lineOfSightChecker;
        private readonly TargetCandidate[] _candidates;
        public ITargetable CurrentTarget { get; private set; }

        public event Action<ITargetable> TargetChanged;

        public TargetingService(
            ITargetCandidateProvider candidateProvider,
            ITargetCandidateSelector candidateSelector,
            ICameraTransformProvider camera,
            TargetingSettingsSO settings,
            ITargetLineOfSightChecker lineOfSightChecker)
        {
            _candidateProvider = candidateProvider;
            _candidateSelector = candidateSelector;
            _cameraProvider = camera;
            _targetingSettings = settings;
            _lineOfSightChecker = lineOfSightChecker;
            _candidates = new TargetCandidate[settings.MaxTargetCandidates];
        }

        public bool TryAcquireFromView()
        {
            var count = _candidateProvider.GetCandidates(_candidates);

            if (!_candidateSelector.TrySelectBest(
                    _candidates,
                    count,
                    CurrentTarget,
                    out var bestCandidate))
            {
                return false;
            }

            if (ReferenceEquals(CurrentTarget, bestCandidate.Targetable))
            {
                ClearTarget();
                return true;
            }

            return TrySetTarget(bestCandidate.Targetable);
        }

        public bool TryCycleTarget(int direction)
        {
            if (direction == 0)
                return false;

            var count = _candidateProvider.GetCandidates(_candidates);
            if (count <= 0)
                return false;

            if (CurrentTarget == null)
            {
                if (!_candidateSelector.TrySelectBest(
                        _candidates,
                        count,
                        null,
                        out var bestCandidate))
                {
                    return false;
                }

                return TrySetTarget(bestCandidate.Targetable);
            }

            Array.Sort(_candidates, 0, count, Comparer<TargetCandidate>.Create(CompareCandidatesForCycle));

            var currentIndex = -1;
            for (var i = 0; i < count; i++)
            {
                if (!ReferenceEquals(_candidates[i].Targetable, CurrentTarget))
                    continue;

                currentIndex = i;
                break;
            }

            if (currentIndex < 0)
            {
                if (!_candidateSelector.TrySelectBest(
                        _candidates,
                        count,
                        null,
                        out var bestCandidate))
                {
                    return false;
                }

                return TrySetTarget(bestCandidate.Targetable);
            }

            var nextIndex = currentIndex + (direction > 0 ? 1 : -1);

            if (nextIndex < 0)
                nextIndex = count - 1;
            else if (nextIndex >= count)
                nextIndex = 0;

            return TrySetTarget(_candidates[nextIndex].Targetable);
        }

        private int CompareCandidatesForCycle(TargetCandidate left, TargetCandidate right)
        {
            var leftAngle = GetSignedHorizontalAngle(left.Position);
            var rightAngle = GetSignedHorizontalAngle(right.Position);

            var angleComparison = leftAngle.CompareTo(rightAngle);
            if (angleComparison != 0)
                return angleComparison;

            return left.Distance.CompareTo(right.Distance);
        }

        private float GetSignedHorizontalAngle(Vector3 targetPosition)
        {
            var origin = _cameraProvider.Position;
            var toTarget = targetPosition - origin;

            var forward = Vector3.ProjectOnPlane(_cameraProvider.Forward, Vector3.up);
            var flatToTarget = Vector3.ProjectOnPlane(toTarget, Vector3.up);

            if (forward.sqrMagnitude <= 0.001f || flatToTarget.sqrMagnitude <= 0.001f)
                return 0f;

            forward.Normalize();
            flatToTarget.Normalize();

            return Vector3.SignedAngle(forward, flatToTarget, Vector3.up);
        }

        public bool TrySetTarget(ITargetable target)
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

        public bool IsValid(ITargetable target)
        {
            if (target == null || !target.IsTargetable || target.AimPoint == null)
                return false;

            var origin = _cameraProvider.Position;
            var toTarget = target.AimPoint.position - origin;
            var distance = toTarget.magnitude;

            if (distance <= 0.001f)
                return false;

            if (distance > _targetingSettings.MaxDistance)
                return false;

            var direction = toTarget / distance;
            var angle = Vector3.Angle(_cameraProvider.Forward, direction);

            if (angle > _targetingSettings.MaxAngle)
                return false;

            var candidate = new TargetCandidate(
                target,
                target.AimPoint.position,
                distance,
                angle);

            return _lineOfSightChecker.HasLineOfSight(candidate);
        }


        public void ClearTarget()
        {
            if (CurrentTarget == null)
                return;

            Debug.Log($"TargetingService: clearing target '{CurrentTarget.DisplayName}'.");
            CurrentTarget = null;
            TargetChanged?.Invoke(null);
        }

        public bool ValidateCurrentTarget()
        {
            if (CurrentTarget == null)
                return false;

            if (IsValid(CurrentTarget))
                return true;

            ClearTarget();
            return false;
        }
    }
}
