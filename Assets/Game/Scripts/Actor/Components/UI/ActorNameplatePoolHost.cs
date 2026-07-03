using UnityEngine;

namespace Game.Actor
{
    public sealed class ActorNameplatePoolHost : MonoBehaviour, IActorNameplatePoolRoots
    {
        [SerializeField] private Transform _activeRoot;
        [SerializeField] private Transform _inactiveRoot;

        public Transform ActiveRoot => _activeRoot != null
            ? _activeRoot
            : transform;

        public Transform InactiveRoot => _inactiveRoot != null
            ? _inactiveRoot
            : transform;
    }
}