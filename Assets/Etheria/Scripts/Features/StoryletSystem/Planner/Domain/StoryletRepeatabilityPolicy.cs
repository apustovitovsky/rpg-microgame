namespace Etheria.Features.StoryletSystem
{
    public sealed class StoryletRepeatabilityPolicy
    {
        public StoryletRepeatabilityPolicy(
            StoryletRepeatabilityMode mode,
            int cooldownSteps = 0)
        {
            Mode = mode;
            CooldownSteps = cooldownSteps;
        }

        public StoryletRepeatabilityMode Mode { get; }
        public int CooldownSteps { get; }

        public static StoryletRepeatabilityPolicy OnceEver()
        {
            return new StoryletRepeatabilityPolicy(StoryletRepeatabilityMode.OnceEver);
        }

        public static StoryletRepeatabilityPolicy OncePerRun()
        {
            return new StoryletRepeatabilityPolicy(StoryletRepeatabilityMode.OncePerRun);
        }

        public static StoryletRepeatabilityPolicy WithCooldown(int cooldownSteps)
        {
            return new StoryletRepeatabilityPolicy(
                StoryletRepeatabilityMode.RepeatableWithCooldown,
                cooldownSteps);
        }

        public static StoryletRepeatabilityPolicy UntilStateChanges()
        {
            return new StoryletRepeatabilityPolicy(StoryletRepeatabilityMode.RepeatableUntilStateChanges);
        }
    }
}
