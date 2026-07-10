using UnityEngine;

namespace Game.Actor
{
    public sealed class TargetNameplatePoolHost : MonoBehaviour, ITargetNameplatePoolRoots
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