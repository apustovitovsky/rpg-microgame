using System.Collections.Generic;

namespace RPG.Gameplay
{
    public interface ITargetDetectionService
    {
        IReadOnlyList<ITargetable> GetCandidates();
    }
}