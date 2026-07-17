using UnityEngine;

namespace Game.Targeting
{
    public sealed class TargetableFilter :
        ITargetFilter
    {
        public bool IsMatch(
            ITargetable target,
            Vector3 origin,
            Vector3 forward)
        {
            return target != null &&
                target.IsTargetable &&
                target.TargetAnchor != null;
        }
    }
}