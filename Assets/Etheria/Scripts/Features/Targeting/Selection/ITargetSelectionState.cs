using Etheria.Game.Targeting;

namespace Etheria.Features.Targeting
{
    public interface ITargetSelectionState : ITargetingEvents
    {
        bool TrySet(ITargetable target);
        void Clear();
    }
}
