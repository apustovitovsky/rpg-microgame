using Game.Targeting;
using UnityEngine;

namespace Game.Actor
{
    [DisallowMultipleComponent]
    public sealed class TargetCapsule : MonoBehaviour
    {
        [SerializeField] private ActorTargetable _targetable;

        public bool TryGetTarget(out ITargetable target)
        {
            target = _targetable;
            return target != null && target.IsTargetable;
        }
    }
}