using Etheria.Game.Targeting;

namespace Etheria.Features.Targeting
{
    public interface ITargetAcquisitionService
    {
        bool TryAcquire(ITargetable currentTarget, out ITargetable target);
    }
}
