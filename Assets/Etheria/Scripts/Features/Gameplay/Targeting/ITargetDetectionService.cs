using System.Collections.Generic;

namespace Etheria.Gameplay
{
    public interface ITargetDetectionService
    {
        IReadOnlyList<ITargetable> GetCandidates();
    }
}