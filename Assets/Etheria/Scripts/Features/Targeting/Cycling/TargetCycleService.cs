using System;
using System.Collections.Generic;
using Etheria.Game.Camera;
using Etheria.Game.Targeting;
using UnityEngine;

namespace Etheria.Features.Targeting
{
    public sealed class TargetCycleService : ITargetCycleService
    {
        private readonly ITargetCandidateSnapshotProvider _snapshotProvider;
        private readonly ITargetCandidateSelector _candidateSelector;
        private readonly ICameraTransformProvider _cameraProvider;

        public TargetCycleService(
            ITargetCandidateSnapshotProvider snapshotProvider,
            ITargetCandidateSelector candidateSelector,
            ICameraTransformProvider cameraProvider)
        {
            _snapshotProvider = snapshotProvider;
            _candidateSelector = candidateSelector;
            _cameraProvider = cameraProvider;
        }


        public bool TryCycle(ITargetable currentTarget, int direction, out ITargetable target)
        {
            target = null;

            if (direction == 0)
                return false;

            var snapshot = _snapshotProvider.Capture();
            var candidates = snapshot.Candidates;
            var count = snapshot.Count;

            if (count <= 0)
                return false;

            if (currentTarget == null)
            {
                if (!_candidateSelector.TrySelectBest(candidates, count, null, out var bestCandidate))
                    return false;

                target = bestCandidate.Targetable;
                return true;
            }

            Array.Sort(candidates, 0, count, Comparer<TargetCandidate>.Create(CompareCandidatesForCycle));

            var currentIndex = -1;
            for (var i = 0; i < count; i++)
            {
                if (!ReferenceEquals(candidates[i].Targetable, currentTarget))
                    continue;

                currentIndex = i;
                break;
            }

            if (currentIndex < 0)
            {
                if (!_candidateSelector.TrySelectBest(candidates, count, null, out var bestCandidate))
                    return false;

                target = bestCandidate.Targetable;
                return true;
            }

            var nextIndex = currentIndex + (direction > 0 ? 1 : -1);

            if (nextIndex < 0)
                nextIndex = count - 1;
            else if (nextIndex >= count)
                nextIndex = 0;

            target = candidates[nextIndex].Targetable;
            return true;
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
    }
}
