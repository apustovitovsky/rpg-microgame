namespace Etheria.Features.Targeting
{
    public interface ITargetEligibilityService
    {
        bool IsEligible(TargetCandidate candidate);
    }
}
