namespace Etheria.Features.StoryletSystem
{
    public sealed class StoryletSaliencePolicy
    {
        public static readonly StoryletSaliencePolicy Default = new(0f, 0f, 3f, 0.5f);

        public StoryletSaliencePolicy(
            float baseWeight,
            float unlockBonus,
            float recentRepeatPenalty,
            float repeatedPairPenalty)
        {
            BaseWeight = baseWeight;
            UnlockBonus = unlockBonus;
            RecentRepeatPenalty = recentRepeatPenalty;
            RepeatedPairPenalty = repeatedPairPenalty;
        }

        public float BaseWeight { get; }
        public float UnlockBonus { get; }
        public float RecentRepeatPenalty { get; }
        public float RepeatedPairPenalty { get; }
    }
}
