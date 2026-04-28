
using UnityEngine;
using VContainer.Unity;

namespace Etheria.Features.Gameplay
{
    [CreateAssetMenu(fileName = "GameplayConfig", menuName = "Etheria/Gameplay/Gameplay Config")]
    public sealed class GameplayConfigSO : ScriptableObject
    {
        [field: SerializeField] public LifetimeScope PlayerPrefab { get; private set; }
        [field: SerializeField] public LifetimeScope DronePrefab { get; private set; }
        [field: SerializeField] public LifetimeScope TurretPrefab { get; private set; }
    }
}

