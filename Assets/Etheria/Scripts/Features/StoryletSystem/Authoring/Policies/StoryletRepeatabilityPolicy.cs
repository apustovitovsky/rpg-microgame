namespace Etheria.Features.StoryletSystem.Authoring
{
    public readonly struct StoryletRepeatabilityPolicy
    {
        public StoryletRepeatabilityPolicy(StoryletRepeatabilityMode mode, int cooldownSteps = 0)
        {
            Mode = mode;
            CooldownSteps = cooldownSteps;
        }

        public StoryletRepeatabilityMode Mode { get; }
        public int CooldownSteps { get; }
    }
}
