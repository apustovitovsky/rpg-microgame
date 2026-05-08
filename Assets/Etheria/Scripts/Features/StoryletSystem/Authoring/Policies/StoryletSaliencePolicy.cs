namespace Etheria.Features.StoryletSystem.Authoring
{
    public readonly struct StoryletSaliencePolicy
    {
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
