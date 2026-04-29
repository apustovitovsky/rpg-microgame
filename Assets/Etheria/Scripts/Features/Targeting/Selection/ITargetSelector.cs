using System.Collections.Generic;
using Etheria.Game.Targeting;

namespace Etheria.Features.Targeting
{
    public interface ITargetSelector
    {
        ITargetable SelectTarget(
            IReadOnlyList<ITargetable> candidates,
            ITargetingService targetingService);
    }
}
