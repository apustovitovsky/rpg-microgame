using Etheria.Game.Targeting;
using UnityEngine;

namespace   Etheria.Features.Targeting
{
    public interface ITargetCandidateResolver
    {
        bool TryResolve(RaycastHit hit, out TargetCandidate candidate);
        bool TryResolve(ITargetable targetable, out TargetCandidate candidate);
    }
}