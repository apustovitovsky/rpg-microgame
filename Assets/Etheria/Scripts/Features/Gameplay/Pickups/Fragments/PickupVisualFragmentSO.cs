using UnityEngine;

namespace Etheria.Features.Gameplay
{
    [CreateAssetMenu(fileName = "PickupVisualFragment", menuName = "Etheria/Gameplay/Pickup/Fragments/PickupVisualFragment")]
    public class PickupVisualFragmentSO : PickupFragmentSO
    {
        [field: Header("Visual Prefab")]
        [field: Tooltip("Visual prefab spawned by WorldPickup when the pickup becomes available.")]
        [field: SerializeField] public GameObject Prefab { get; private set; }

        [field: Header("Local Transform")]
        [field: Tooltip("Local position applied to the spawned prefab relative to the spawn anchor.")]
        [field: SerializeField] public Vector3 LocalPosition { get; private set; }

        [field: Tooltip("Local rotation in Euler angles applied to the spawned prefab.")]
        [field: SerializeField] public Vector3 LocalRotationEuler { get; private set; }

        [field: Tooltip("Local scale applied to the spawned prefab.")]
        [field: SerializeField] public Vector3 LocalScale { get; private set; } = Vector3.one;
    }
}

