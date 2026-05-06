using Etheria.Game.Camera;
using Etheria.Game.Targeting;
using UnityEngine;

namespace Etheria.Features.Targeting
{
    public sealed class TargetCandidateResolver : ITargetCandidateResolver
    {
        private readonly ICameraTransformProvider _camera;

        public TargetCandidateResolver(ICameraTransformProvider camera)
        {
            _camera = camera;
        }

        public bool TryResolve(RaycastHit hit, out TargetCandidate candidate)
        {
            candidate = default;

            if (!hit.collider.TryGetComponent<ITargetableProvider>(out var hitbox))
                return false;

            return TryResolve(hitbox.Targetable, out candidate);
        }

        public bool TryResolve(ITargetable targetable, out TargetCandidate candidate)
        {
            candidate = default;

            if (targetable == null || !targetable.IsTargetable)
                return false;

            var aimPoint = targetable.AimPoint;
            if (aimPoint == null)
                return false;

            var toTarget = aimPoint.position - _camera.Position;
            var distance = toTarget.magnitude;

            if (distance <= 0.001f)
                return false;

            var angle = Vector3.Angle(_camera.Forward, toTarget / distance);

            candidate = new TargetCandidate(
                targetable,
                aimPoint.position,
                distance,
                angle);

            return true;
        }
    }
}