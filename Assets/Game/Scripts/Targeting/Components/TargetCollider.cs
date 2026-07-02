using System;
using UnityEngine;

namespace Game.Targeting
{
    [DisallowMultipleComponent]
    public sealed class TargetCollider : MonoBehaviour
    {
        [SerializeField] private Targetable _targetable;

        public bool TryGetTarget(out ITargetable target)
        {
            target = _targetable;
            return target != null && target.IsTargetable;
        }
    }
}