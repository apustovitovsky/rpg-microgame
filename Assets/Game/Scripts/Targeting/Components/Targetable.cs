using UnityEngine;

namespace Game.Targeting
{
    [DisallowMultipleComponent]
    public sealed class Targetable :
        MonoBehaviour,
        ITargetable
    {
        [SerializeField] private Transform _root;
        [SerializeField] private Transform _targetPoint;
        [SerializeField] private bool _isTargetable = true;

        public Transform Root => _root != null
            ? _root
            : transform;

        public Transform TargetPoint => _targetPoint != null
            ? _targetPoint
            : Root;

        public bool IsTargetable => _isTargetable;
    }
}