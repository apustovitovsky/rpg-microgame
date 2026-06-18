using Etheria.Game.Targeting;

namespace Etheria.Features.Targeting
{
    public interface ITargetValidator
    {
        bool IsValid(ITargetable target);
    }
}