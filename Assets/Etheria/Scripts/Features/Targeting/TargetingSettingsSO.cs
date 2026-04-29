using UnityEngine;

namespace Etheria.Features.Targeting
{
    [CreateAssetMenu(
        fileName = "TargetingSettings",
        menuName = "Etheria/Gameplay/Targeting/Targeting Settings")]
    public sealed class TargetingSettingsSO : ScriptableObject
    {
        [field: SerializeField] public float MaxDistance { get; private set; } = 25f;
        [field: SerializeField] public LayerMask TargetingMask { get; private set; }
        [field: SerializeField] public LayerMask VisibilityMask { get; private set; }
        [field: SerializeField] public int MaxHitResults { get; private set; } = 64;
        [field: SerializeField] public int MaxTargetCandidates { get; private set; } = 16;
        [field: SerializeField] public float Radius { get; private set; } = 2.5f;
        [field: SerializeField] public float MaxAngle { get; private set; } = 30f;
        [field: SerializeField] public float AngleWeight { get; private set; } = 1f;
        [field: SerializeField] public float DistanceWeight { get; private set; } = 0.35f;
        [field: SerializeField] public float CurrentTargetBonus { get; private set; } = 0.5f;
        [field: SerializeField] public float TargetSwitchThreshold { get; private set; } = 0.15f;
    }
}
