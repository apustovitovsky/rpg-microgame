using Game.Targeting;
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

        public string TargetId => _identity != null
            ? _identity.InstanceId
            : string.Empty;

        public Transform Root => _root != null
            ? _root
            : transform;

        public Transform TargetPoint => _targetPoint != null
            ? _targetPoint
            : Root;

        public bool IsTargetable =>
            _isTargetable &&
            !string.IsNullOrWhiteSpace(TargetId);

        [Inject]
        public void Construct(IActorIdentity identity)
        {
            _identity = identity;
        }
    }
}