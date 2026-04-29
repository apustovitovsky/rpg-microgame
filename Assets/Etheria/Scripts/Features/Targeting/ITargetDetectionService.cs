using System.Collections.Generic;
using Etheria.Game.Targeting;

namespace Etheria.Features.Targeting
{
    public interface ITargetDetectionService
    {
        IReadOnlyList<ITargetable> GetCandidates();
    }
}
