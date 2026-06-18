
using Etheria.Game.Camera;
using Etheria.Game.Targeting;
using UnityEngine;

namespace Etheria.Features.Targeting
{

    public interface ITargetLineOfSightChecker
    {
        bool HasLineOfSight(TargetCandidate candidate);
    }

    public sealed class TargetLineOfSightChecker : ITargetLineOfSightChecker
    {
        private readonly ICameraTransformProvider _camera;
        private readonly TargetingSettingsSO _settings;

        public TargetLineOfSightChecker(
            ICameraTransformProvider camera,
            TargetingSettingsSO settings)
        {
            _camera = camera;
            _settings = settings;
        }

        public bool HasLineOfSight(TargetCandidate candidate)
        {
            var origin = _camera.Position;
            var targetPosition = candidate.Position;

            var direction = targetPosition - origin;
            var distance = direction.magnitude;

            if (distance <= 0.001f)
                return false;

            direction /= distance;

            if (!Physics.Raycast(
                    origin,
                    direction,
                    out var hit,
                    distance,
                    _settings.VisibilityMask,
                    QueryTriggerInteraction.Ignore))
            {
                return true;
            }

            return hit.collider.TryGetComponent<ITargetableProvider>(out var hitbox)
                && ReferenceEquals(hitbox.Targetable, candidate.Targetable);
        }
    }
}