using Game.Targeting;
using Game.World;
using UnityEngine;
using VContainer;

namespace Game.Actor
{
    [DisallowMultipleComponent]
    public sealed class ActorTarget :
        MonoBehaviour,
        ITargetable
    {
        [SerializeField] private Transform _uiAnchor;
        [SerializeField] private Transform _targetPoint;
        [SerializeField] private bool _isTargetable = true;

        public WorldInfo Info { get; private set; }

        public WorldId WorldId => Info.WorldId;

        public Transform UiAnchor => _uiAnchor != null
            ? _uiAnchor
            : transform;

        public Transform TargetPoint => _targetPoint != null
            ? _targetPoint
            : transform;

        public bool IsTargetable =>
            _isTargetable &&
            !WorldId.IsEmpty;

        [Inject]
        public void Construct(WorldInfo worldInfo)
        {
            Info = worldInfo;
        }
    }
}