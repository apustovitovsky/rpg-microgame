using UnityEngine;

namespace Etheria.Features.Gameplay
{
    [CreateAssetMenu(
        fileName = "TargetingSettings",
        menuName = "Etheria/Gameplay/Targeting/Targeting Settings")]
    public sealed class TargetingSettingsSO : ScriptableObject
    {
        [field: SerializeField] public float MaxDistance { get; private set; } = 25f;
        [field: SerializeField] public float MaxViewAngle { get; private set; } = 30f;
        [field: SerializeField] public LayerMask TargetingMask { get; private set; }
        [field: SerializeField] public LayerMask ObstructionMask { get; private set; }
    }
}
