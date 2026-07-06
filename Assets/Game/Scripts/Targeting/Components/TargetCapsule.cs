using UnityEngine;

namespace Game.Targeting
{
    [DisallowMultipleComponent]
    public sealed class TargetCapsule : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour _targetable;

        public bool TryGetTarget(out ITargetable target)
        {
            target = _targetable as ITargetable;
            return target != null && target.IsTargetable;
        }
    }
}