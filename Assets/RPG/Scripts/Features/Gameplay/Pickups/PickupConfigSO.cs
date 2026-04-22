using Unity.Cinemachine;
using UnityEngine;
using VContainer.Unity;

namespace RPG.Gameplay
{
    [CreateAssetMenu(fileName = "PickupConfig", menuName = "RPG/Gameplay/Pickups/Pickup Config")]
    public sealed class PickupConfigSO : ScriptableObject
    {
        [field: SerializeField] public Pickup PickupPrefab { get; private set; }

    }
}