using UnityEngine;

namespace Etheria.Features.Targeting
{
    public readonly struct TargetCandidate
    {
        public readonly ITargetable Targetable;
        public readonly float Distance;
        public readonly float Angle;
        public readonly Vector3 Position;

        public TargetCandidate(
            ITargetable targetable,
            Vector3 point,
            float distance,
            float angle)
        {
            Targetable = targetable;
            Position = point;
            Distance = distance;
            Angle = angle;
        }
    }
}