using System.Collections.Generic;

namespace RPG.Gameplay
{
    public sealed class NullTargetDetectionService : ITargetDetectionService
    {
        private static readonly IReadOnlyList<IActorTargetable> EmptyCandidates = new List<IActorTargetable>(0);

        public IReadOnlyList<IActorTargetable> GetCandidates()
        {
            return EmptyCandidates;
        }
    }
}