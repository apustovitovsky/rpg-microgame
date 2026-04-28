using System.Collections.Generic;

namespace Etheria.Features.Targeting
{
    public interface ITargetSelector
    {
        ITargetable SelectTarget(
            IReadOnlyList<ITargetable> candidates,
            ITargetingService targetingService);
    }
}
