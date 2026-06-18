namespace Etheria.Features.Targeting
{
    public interface ITargetCandidateValidityFilter
    {
        int FilterInPlace(TargetCandidate[] candidates, int count);
    }
}
