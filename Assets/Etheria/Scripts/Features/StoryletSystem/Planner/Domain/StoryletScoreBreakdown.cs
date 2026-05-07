namespace Etheria.Features.StoryletSystem
{
    public sealed class StoryletScoreBreakdown
    {
        public StoryletScoreBreakdown(
            float instantiationQuality,
            float transitionValue,
            float futurePotential,
            float antiRepetition)
        {
            InstantiationQuality = instantiationQuality;
            TransitionValue = transitionValue;
            FuturePotential = futurePotential;
            AntiRepetition = antiRepetition;
        }

        public float InstantiationQuality { get; }
        public float TransitionValue { get; }
        public float FuturePotential { get; }
        public float AntiRepetition { get; }
        public float Total => InstantiationQuality + TransitionValue + FuturePotential + AntiRepetition;
    }
}
