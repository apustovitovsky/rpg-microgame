
using UnityEngine;
using VContainer.Unity;

namespace Etheria.Features
{
    [CreateAssetMenu(fileName = "GameplayConfig", menuName = "Etheria/Gameplay/Gameplay Config")]
    public sealed class GameplayConfigSO : ScriptableObject
    {
        [field: SerializeField] public LifetimeScope PlayerPrefab { get; private set; }
        [field: SerializeField] public LifetimeScope DronePrefab { get; private set; }
        [field: SerializeField] public LifetimeScope TurretPrefab { get; private set; }
        [field: SerializeField, Min(0)] public int AdditionalPlayersCount { get; private set; } = 5;
        [field: SerializeField, Min(0f)] public float AdditionalPlayersSpawnRadius { get; private set; } = 10f;
    }
}
