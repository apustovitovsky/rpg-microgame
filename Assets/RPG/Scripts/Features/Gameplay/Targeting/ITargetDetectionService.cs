using System.Collections.Generic;

namespace RPG.Gameplay
{
    public interface ITargetDetectionService
    {
        IReadOnlyList<IActorTargetable> GetCandidates();
    }
}