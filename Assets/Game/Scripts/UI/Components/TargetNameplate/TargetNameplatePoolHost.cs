using UnityEngine;

namespace Game.UI
{
    public interface ITargetNameplatePoolRoots
    {
        Transform ActiveRoot { get; }

        Transform InactiveRoot { get; }
    }

    public sealed class TargetNameplatePoolHost : MonoBehaviour, ITargetNameplatePoolRoots
    {
        [SerializeField] private Transform _activeRoot;
        [SerializeField] private Transform _inactiveRoot;

        public Transform ActiveRoot => _activeRoot != null ? _activeRoot : transform;

        public Transform InactiveRoot => _inactiveRoot != null ? _inactiveRoot : transform;
    }
}