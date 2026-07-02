using UnityEngine;

namespace Game.Actor
{
    [CreateAssetMenu(
        fileName = "TargetingConfig",
        menuName = "Game/Actor/Targeting Config")]
    public sealed class TargetingConfigSO : ScriptableObject
    {
        [field: SerializeField]
        public float DistanceScoreWeight { get; private set; } = 100f;

        [field: SerializeField]
        public float AngleScoreWeight { get; private set; } = 40f;
    }
}