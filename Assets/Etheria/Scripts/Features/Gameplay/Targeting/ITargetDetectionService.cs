using System.Collections.Generic;

namespace Etheria.Features.Gameplay
{
    public interface ITargetDetectionService
    {
        IReadOnlyList<ITargetable> GetCandidates();
    }
}
