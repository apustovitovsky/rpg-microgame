namespace Etheria.Features.StoryletSystem
{
    public sealed class StoryletSalienceEvaluation
    {
        public StoryletSalienceEvaluation(float bonus, float antiRepetitionPenalty)
        {
            Bonus = bonus;
            AntiRepetitionPenalty = antiRepetitionPenalty;
        }

        public float Bonus { get; }
        public float AntiRepetitionPenalty { get; }
    }
}
