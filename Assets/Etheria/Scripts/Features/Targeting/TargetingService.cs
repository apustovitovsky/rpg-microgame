using System;
using Etheria.Game.Camera;
using UnityEngine;

namespace Etheria.Features.Targeting
{
    public sealed class TargetingService : ITargetingService
    {
        private readonly ITargetDetectionService _targetDetectionService;
        private readonly ICameraTransformProvider _cameraProvider;
        private readonly TargetingSettingsSO _targetingSettings;

        public ITargetable CurrentTarget { get; private set; }

        public event Action<ITargetable> TargetChanged;

        public TargetingService(ITargetDetectionService targetDetectionService)
        {
            _targetDetectionService = targetDetectionService;
        }

        public TargetingService(
            ITargetDetectionService targetDetectionService,
            ICameraTransformProvider camera,
            TargetingSettingsSO settings)
        {
            _targetDetectionService = targetDetectionService;
            _cameraProvider = camera;
            _targetingSettings = settings;
        }

        public bool TryAcquireFromView()
        {
            var candidates = _targetDetectionService.GetCandidates();
            for (var i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                // Debug.Log($"Target acquired from view: {candidate.DisplayName}");
                if (!TrySetTarget(candidate))
                    continue;

                return true;
            }
            // Debug.Log("Failed to acquire target from view");
            return false;
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

