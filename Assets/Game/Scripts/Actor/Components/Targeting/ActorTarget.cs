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
        [SerializeField] private Transform _root;
        [SerializeField] private Transform _targetPoint;
        [SerializeField] private bool _isTargetable = true;

        public WorldId WorldId { get; private set; }

        public Transform Root => _root != null
            ? _root
            : transform;

        public Transform TargetPoint => _targetPoint != null
            ? _targetPoint
            : Root;

        public bool IsTargetable =>
            _isTargetable &&
            !WorldId.IsEmpty;

        // public void Initialize(WorldId worldId)
        // {
        //     WorldId = worldId;
        // }

        [Inject]
        public void Construct(WorldId worldId)
        {
            WorldId = worldId;
        }
    }
}