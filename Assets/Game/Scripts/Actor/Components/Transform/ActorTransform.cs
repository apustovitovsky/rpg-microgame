using UnityEngine;

namespace Game.Actor
{
    [DisallowMultipleComponent]
    public sealed class ActorTransform :
        MonoBehaviour,
        IActorTransform
    {
        public Transform Root => transform;

        [SerializeField] private Transform _cameraPivot;
        public Transform CameraPivot => _cameraPivot != null
            ? _cameraPivot
            : Root;
    }
}