using Game.Targeting;
using Game.World;
using UnityEngine;

namespace Game.Pickup
{
    [DisallowMultipleComponent]
    public sealed class PickupTarget :
        MonoBehaviour,
        ITargetable
    {
        [SerializeField] private WorldPickup _pickup;
        [SerializeField] private Transform _root;
        [SerializeField] private Transform _targetPoint;
        [SerializeField] private bool _isTargetable = true;

        public WorldId WorldId => _pickup != null
            ? _pickup.WorldId
            : default;

        public Transform Root => _root != null
            ? _root
            : transform;

        public Transform TargetPoint => _targetPoint != null
            ? _targetPoint
            : Root;

        public bool IsTargetable =>
            _isTargetable &&
            _pickup != null;
    }
}