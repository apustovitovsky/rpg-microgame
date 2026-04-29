namespace Etheria.Features.Targeting
{


    public interface ITargetCandidateComparer
    {
        bool IsBetter(
            TargetCandidate candidate,
            float candidateScore,
            TargetCandidate currentBest,
            float currentBestScore,
            ITargetable currentTarget);
    }


    public sealed class TargetCandidateComparer : ITargetCandidateComparer
    {
        private const float ScoreEpsilon = 0.001f;

        public bool IsBetter(
            TargetCandidate candidate,
            float candidateScore,
            TargetCandidate currentBest,
            float currentBestScore,
            ITargetable currentTarget)
        {
            if (candidateScore > currentBestScore + ScoreEpsilon)
                return true;

            if (candidateScore < currentBestScore - ScoreEpsilon)
                return false;

            if (ReferenceEquals(candidate.Targetable, currentTarget))
                return true;

            if (ReferenceEquals(currentBest.Targetable, currentTarget))
                return false;

            if (candidate.Angle < currentBest.Angle - ScoreEpsilon)
                return true;

            if (candidate.Angle > currentBest.Angle + ScoreEpsilon)
                return false;

            return candidate.Distance < currentBest.Distance;
        }
    }
}