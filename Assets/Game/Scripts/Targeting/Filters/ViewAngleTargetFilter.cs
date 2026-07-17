using UnityEngine;

namespace Game.Targeting
{
    public sealed class ViewAngleTargetFilter :
        ITargetFilter
    {
        private readonly float _minimumDot;

        public ViewAngleTargetFilter(
            float maximumAngle)
        {
            maximumAngle = Mathf.Clamp(
                maximumAngle,
                0f,
                180f);

            _minimumDot = Mathf.Cos(
                maximumAngle * Mathf.Deg2Rad);
        }

        public bool IsMatch(
            ITargetable target,
            Vector3 origin,
            Vector3 forward)
        {
            var direction =
                target.TargetAnchor.position - origin;

            direction.y = 0f;
            forward.y = 0f;

            if (direction.sqrMagnitude <= 0.0001f ||
                forward.sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            return Vector3.Dot(
                forward.normalized,
                direction.normalized) >= _minimumDot;
        }
    }
}