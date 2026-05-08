using System;
using UnityEngine;

namespace Etheria.Features.StoryletSystem.Authoring
{
    [Serializable]
    public sealed class RepeatabilityPolicyAuthoring
    {
        [SerializeField]
        private StoryletRepeatabilityMode _mode = StoryletRepeatabilityMode.OncePerRun;

        [SerializeField]
        private int _cooldownSteps;

        public StoryletRepeatabilityMode Mode => _mode;
        public int CooldownSteps => _cooldownSteps;
    }
}
