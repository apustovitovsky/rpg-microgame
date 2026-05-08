using System;
using UnityEngine;

namespace Etheria.Features.StoryletSystem.Authoring
{
    [Serializable]
    public sealed class SaliencePolicyAuthoring
    {
        [SerializeField] private float _baseWeight;
        [SerializeField] private float _unlockBonus;
        [SerializeField] private float _recentRepeatPenalty = 3f;
        [SerializeField] private float _repeatedPairPenalty = 0.5f;

        public float BaseWeight => _baseWeight;
        public float UnlockBonus => _unlockBonus;
        public float RecentRepeatPenalty => _recentRepeatPenalty;
        public float RepeatedPairPenalty => _repeatedPairPenalty;
    }
}
