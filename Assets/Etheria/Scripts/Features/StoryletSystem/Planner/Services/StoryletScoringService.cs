namespace Etheria.Features.StoryletSystem
{
    public sealed class StoryletScoringService : IStoryletScoringService
    {
        public StoryletScoreBreakdown Evaluate(
            StoryletInstantiationCandidate candidate,
            StoryletWorldState currentWorldState,
            StoryletWorldState nextWorldState,
            StoryletPlannerMemory memory,
            int futureCandidateCount)
        {
            var transitionValue = candidate.Definition.Priority * 6f
                + candidate.Salience.Bonus * 3f
                + candidate.EffectPreview.Effects.Count * 1f;
            var futurePotential = futureCandidateCount * 3f;
            var antiRepetition = candidate.Salience.AntiRepetitionPenalty;

            return new StoryletScoreBreakdown(
                candidate.InstantiationQuality,
                transitionValue,
                futurePotential,
                antiRepetition);
        }
    }
}
