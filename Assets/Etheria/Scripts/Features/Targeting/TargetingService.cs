using System;
using Etheria.Game.Camera;
using UnityEngine;

namespace Etheria.Features.Targeting
{
    public sealed class TargetingService : ITargetingService
    {
        private readonly ITargetDetectionService _targetDetectionService;
        private readonly ITargetSelector _targetSelector;
        private readonly ICameraTransformProvider _cameraProvider;
        private readonly TargetingSettingsSO _targetingSettings;

        public ITargetable CurrentTarget { get; private set; }

        public event Action<ITargetable> TargetChanged;

        public TargetingService(
            ITargetDetectionService targetDetectionService,
            ITargetSelector targetSelector,
            ICameraTransformProvider camera,
            TargetingSettingsSO settings)
        {
            _targetDetectionService = targetDetectionService;
            _targetSelector = targetSelector;
            _cameraProvider = camera;
            _targetingSettings = settings;
        }

        public bool TryAcquireFromView()
        {
            var candidates = _targetDetectionService.GetCandidates();
            for (var candidateIndex = 0; candidateIndex < candidates.Count; candidateIndex++)
            {
                Debug.LogWarning($"TargetingService: candidate {candidateIndex}: {candidates[candidateIndex].DisplayName}");
            }
            var selectedTarget = _targetSelector.SelectTarget(candidates, this);
            if (selectedTarget == null)
                return false;

            return TrySetTarget(selectedTarget);
        }

        public bool TrySetTarget(ITargetable target)
        {
            if (!IsValid(target))
                return false;

            if (ReferenceEquals(CurrentTarget, target))
            {
                Debug.Log($"TargetingService: target '{target.DisplayName}' is already current. Clearing it.");
                ClearTarget();
                return true;
            }

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

            var sqrDistance = toTarget.sqrMagnitude;
            if (sqrDistance > _targetingSettings.MaxDistance * _targetingSettings.MaxDistance)
                return false;

            var angle = Vector3.Angle(_cameraProvider.Forward, toTarget);
            if (angle > _targetingSettings.MaxViewAngle)
                return false;

            var distance = Mathf.Sqrt(sqrDistance);
            var direction = toTarget / distance;

            if (Physics.Raycast(origin, direction, out var _, distance, _targetingSettings.ObstructionMask))
            {
                return false;
            }

            return true;
        }


        public void ClearTarget()
        {
            if (CurrentTarget == null)
            {
                // Debug.Log("TargetingService: ClearTarget skipped because CurrentTarget is already null.");
                return;
            }

            Debug.Log($"TargetingService: clearing target '{CurrentTarget.DisplayName}'.");
            CurrentTarget = null;

            // Debug.Log("TargetingService: invoking TargetChanged with null.");
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

