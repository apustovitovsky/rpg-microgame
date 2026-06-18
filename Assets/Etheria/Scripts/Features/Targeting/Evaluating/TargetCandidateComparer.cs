using Etheria.Game.Targeting;

namespace Etheria.Features.Targeting
{
    public sealed class TargetCandidateComparer : ITargetCandidateComparer
    {
        private const float ScoreEpsilon = 0.001f;

        private readonly TargetingSettingsSO _settings;

        public TargetCandidateComparer(TargetingSettingsSO settings)
        {
            _settings = settings;
        }

        public bool IsBetter(
            TargetCandidate candidate,
            float candidateScore,
            TargetCandidate currentBest,
            float currentBestScore,
            ITargetable currentTarget)
        {
            var candidateIsCurrent = ReferenceEquals(candidate.Targetable, currentTarget);
            var bestIsCurrent = ReferenceEquals(currentBest.Targetable, currentTarget);

            if (!candidateIsCurrent &&
                bestIsCurrent &&
                candidateScore < currentBestScore + _settings.TargetSwitchThreshold)
            {
                return false;
            }

            if (candidateScore > currentBestScore + ScoreEpsilon)
                return true;

            if (candidateScore < currentBestScore - ScoreEpsilon)
                return false;

            if (candidateIsCurrent)
                return true;

            if (bestIsCurrent)
                return false;

            if (candidate.Angle < currentBest.Angle - ScoreEpsilon)
                return true;

            if (candidate.Angle > currentBest.Angle + ScoreEpsilon)
                return false;

            return candidate.Distance < currentBest.Distance;
        }
    }
}
