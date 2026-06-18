using Etheria.Game.Targeting;

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
}