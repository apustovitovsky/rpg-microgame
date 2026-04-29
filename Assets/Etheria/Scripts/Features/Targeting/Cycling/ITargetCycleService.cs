using Etheria.Game.Targeting;

namespace Etheria.Features.Targeting
{
    public interface ITargetCycleService
    {
        bool TryCycle(ITargetable currentTarget, int direction, out ITargetable target);
    }
}
