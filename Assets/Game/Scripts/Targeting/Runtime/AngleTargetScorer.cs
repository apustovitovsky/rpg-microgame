using UnityEngine;

namespace Game.Targeting
{
    public sealed class AngleTargetScorer :
        ITargetScorer
    {
        private readonly float _weight;

        public AngleTargetScorer(float weight)
        {
            _weight = weight;
        }

        public float Score(
            ITargetable target,
            Vector3 origin,
            Vector3 forward)
        {
            Vector3 direction = target.TargetPoint.position - origin;

            if (direction.sqrMagnitude <= 0.0001f)
                return 0f;

            return Vector3.Dot(
                direction.normalized,
                forward.normalized) * _weight;
        }
    }
}