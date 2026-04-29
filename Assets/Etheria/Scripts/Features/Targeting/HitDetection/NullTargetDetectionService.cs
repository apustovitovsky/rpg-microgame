using System.Collections.Generic;
using Etheria.Game.Targeting;

namespace Etheria.Features.Targeting
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
