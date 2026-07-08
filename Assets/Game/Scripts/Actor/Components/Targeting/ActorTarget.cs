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

        private IWorldActor _actor;

        public WorldId WorldId => _actor != null
            ? _actor.WorldId
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
        public void Construct(IWorldActor actor)
        {
            _actor = actor;
        }
    }
}