using Game.World;
using UnityEngine;

namespace Game.Targeting
{
    [DisallowMultipleComponent]
    public sealed class Targetable :
        MonoBehaviour,
        ITargetable
    {
        [SerializeField] private Transform _uiAnchor;
        [SerializeField] private Transform _targetAnchor;
        [SerializeField] private bool _isTargetable = true;

        public WorldInfo Info { get; private set; }

        public WorldId WorldId => Info.WorldId;

        public Transform UiAnchor => _uiAnchor != null
            ? _uiAnchor
            : transform;

        public Transform TargetAnchor => _targetAnchor != null
            ? _targetAnchor
            : transform;

        public bool IsTargetable =>
            _isTargetable &&
            !WorldId.IsEmpty;

        public void Initialize(WorldInfo info)
        {
            Info = info;
        }
    }
}