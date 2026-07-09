using UnityEngine;

namespace Game.Targeting
{
    public sealed class DistanceTargetScorer :
        ITargetScorer
    {
        private readonly float _weight;

        public DistanceTargetScorer(float weight)
        {
            _weight = weight;
        }

        public float Score(
            ITargetable target,
            Vector3 origin,
            Vector3 forward)
        {
            float distance = Vector3.Distance(
                origin,
                target.TargetAnchor.position);

            return _weight / Mathf.Max(distance, 0.01f);
        }
    }
}