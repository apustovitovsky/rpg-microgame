using Etheria.Game.Targeting;
using UnityEngine;

namespace Etheria.Features.Targeting
{
    public interface ITargetCandidateEvaluator
    {
        float Evaluate(TargetCandidate candidate, ITargetable currentTarget);
    }
}
