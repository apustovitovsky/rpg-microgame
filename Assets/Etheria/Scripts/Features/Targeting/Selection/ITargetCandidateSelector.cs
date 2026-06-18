using Etheria.Game.Targeting;

namespace Etheria.Features.Targeting
{
    public interface ITargetCandidateSelector
    {
        bool TrySelectBest(
            TargetCandidate[] candidates,
            int count,
            ITargetable currentTarget,
            out TargetCandidate bestCandidate);
    }
}
