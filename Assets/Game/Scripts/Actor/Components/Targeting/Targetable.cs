using Game.Targeting;
using UnityEngine;

namespace Game.Actor
{
    [DisallowMultipleComponent]
    public sealed class Targetable :
        MonoBehaviour,
        ITargetable
    {
        [SerializeField] private ActorView _actorView;

        public string TargetId => _actorView != null
            ? _actorView.ActorId
            : string.Empty;

        public Transform Root => _actorView != null
            ? _actorView.Root
            : transform;

        public Transform TargetPoint => _actorView != null
            ? _actorView.TargetPoint
            : transform;

        public bool IsTargetable => _actorView != null &&
            !string.IsNullOrWhiteSpace(_actorView.ActorId);
    }
}