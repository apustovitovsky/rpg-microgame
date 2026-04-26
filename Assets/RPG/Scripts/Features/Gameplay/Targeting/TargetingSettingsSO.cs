using UnityEngine;

namespace RPG.Gameplay
{
    [CreateAssetMenu(
        fileName = "TargetingSettings",
        menuName = "RPG/Gameplay/Targeting/Targeting Settings")]
    public sealed class TargetingSettingsSO : ScriptableObject
    {
        [field: SerializeField] public float MaxDistance { get; private set; } = 25f;
    }
}