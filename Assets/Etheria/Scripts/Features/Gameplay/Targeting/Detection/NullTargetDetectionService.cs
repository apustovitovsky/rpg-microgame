using System.Collections.Generic;

namespace Etheria.Features.Gameplay
{
    public sealed class NullTargetDetectionService : ITargetDetectionService
    {
        private static readonly IReadOnlyList<ITargetable> EmptyCandidates = new List<ITargetable>(0);

        public IReadOnlyList<ITargetable> GetCandidates()
        {
            return EmptyCandidates;
        }
    }
}
