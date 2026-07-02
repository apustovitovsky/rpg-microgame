using UnityEngine;

namespace Game.Targeting
{
    public interface ITargetScorer
    {
        float Score(
            ITargetable target,
            Vector3 origin,
            Vector3 forward);
    }
}