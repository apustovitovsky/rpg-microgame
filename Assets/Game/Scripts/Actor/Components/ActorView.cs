
using UnityEngine;

namespace Game.Actor
{
    [DisallowMultipleComponent]
    public sealed class ActorView :
        MonoBehaviour,
        IActorView
    {
        [SerializeField] private Transform _cameraPivot;
        [SerializeField] private Transform _targetPoint;
        [SerializeField] private Transform _uiAnchor;

        public Transform Root => transform;

        public Transform TargetPoint => _targetPoint != null
            ? _targetPoint
            : Root;

        public Transform CameraPivot => _cameraPivot != null
            ? _cameraPivot
            : Root;

        public Transform UiAnchor => _uiAnchor != null
            ? _uiAnchor
            : Root;
    }
}