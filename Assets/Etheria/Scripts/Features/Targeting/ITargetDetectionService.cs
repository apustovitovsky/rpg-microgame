using System.Collections.Generic;

namespace Etheria.Features.Targeting
{
    public interface ITargetDetectionService
    {
        IReadOnlyList<ITargetable> GetCandidates();
    }
}
