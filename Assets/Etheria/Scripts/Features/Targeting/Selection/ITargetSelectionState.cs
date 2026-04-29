using Etheria.Game.Targeting;

namespace Etheria.Features.Targeting
{
    public interface ITargetSelectionState : ITargetingEvents
    {
        TargetSelectionResult Set(ITargetable target);
        TargetSelectionResult Clear();
    }
}

