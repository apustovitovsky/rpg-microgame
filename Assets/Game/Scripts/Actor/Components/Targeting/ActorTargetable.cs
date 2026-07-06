using Game.Targeting;
using Game.World;
using UnityEngine;
using VContainer;

namespace Game.Actor
{
    [DisallowMultipleComponent]
    public sealed class ActorTargetable :
        MonoBehaviour,
        ITargetable
    {
        [SerializeField] private Transform _root;
        [SerializeField] private Transform _targetPoint;
        [SerializeField] private bool _isTargetable = true;

        private IActorIdentity _identity;

        public WorldId WorldId => _identity != null
            ? _identity.WorldId
            : default;

        public Transform Root => _root != null
            ? _root
            : transform;

        public Transform TargetPoint => _targetPoint != null
            ? _targetPoint
            : Root;

        public bool IsTargetable =>
            _isTargetable &&
            !WorldId.IsEmpty;

        [Inject]
        public void Construct(IActorIdentity identity)
        {
            _identity = identity;
        }
    }
}