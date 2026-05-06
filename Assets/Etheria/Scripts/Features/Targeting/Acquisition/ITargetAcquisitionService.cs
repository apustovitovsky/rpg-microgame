using Etheria.Game.Targeting;


namespace Etheria.Features.Targeting
{
    public interface ITargetAcquisitionService
    {
        TargetAcquireResult Acquire(ITargetable currentTarget);
    }
}

