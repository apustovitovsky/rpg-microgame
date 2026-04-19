using Unity.Cinemachine;
using UnityEngine;
using VContainer.Unity;

namespace RPG.Gameplay
{
    [CreateAssetMenu(fileName = "GameplayConfig", menuName = "RPG/Gameplay/Gameplay Config")]
    public sealed class GameplayConfigSO : ScriptableObject
    {
        [field: SerializeField] public LifetimeScope PlayerPrefab { get; private set; }
        [field: SerializeField] public LifetimeScope DronePrefab { get; private set; }
        [field: SerializeField] public LifetimeScope TurretPrefab { get; private set; }
        [field: SerializeField] public string WorldScene { get; private set; }
        [field: SerializeField] public CinemachineCamera VirtualCamera { get; private set; }
    }
}