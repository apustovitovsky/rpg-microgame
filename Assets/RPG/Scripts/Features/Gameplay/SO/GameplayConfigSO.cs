using Unity.Cinemachine;
using UnityEngine;

namespace RPG.Gameplay
{
    [CreateAssetMenu(fileName = "GameplayConfig", menuName = "RPG/Gameplay/Gameplay Config")]
    public sealed class GameplayConfigSO : ScriptableObject
    {
        [field: SerializeField] public GameObject PlayerPrefab { get; private set; }
        [field: SerializeField] public GameObject DronePrefab { get; private set; }
        [field: SerializeField] public GameObject TurretPrefab { get; private set; }
        [field: SerializeField] public CinemachineCamera VirtualCamera { get; private set; }
    }
}
