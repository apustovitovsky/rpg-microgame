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
        [SerializeField] private PickupComponent _pickup;
        [SerializeField] private Transform _uiAnchor;
        [SerializeField] private Transform _targetPoint;
        [SerializeField] private bool _isTargetable = true;

        public WorldInfo Info =>
            _pickup != null && _pickup.Pickup != null
                ? _pickup.Pickup.Info
                : default;

        public WorldId WorldId => Info.WorldId;

        public Transform UiAnchor => _uiAnchor != null
            ? _uiAnchor
            : transform;

        public Transform TargetPoint => _targetPoint != null
            ? _targetPoint
            : transform;

        public bool IsTargetable =>
            _isTargetable &&
            _pickup != null &&
            _pickup.Pickup != null &&
            !WorldId.IsEmpty;
    }
}