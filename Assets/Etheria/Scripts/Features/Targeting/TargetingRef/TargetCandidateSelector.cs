
using Etheria.Game.Targeting;

namespace Etheria.Features.Targeting
{
    public interface ITargetCandidateSelector
    {
        bool TrySelectBest(
            TargetCandidate[] candidates,
            int count,
            ITargetable currentTarget,
            out TargetCandidate bestCandidate);
    }

    public sealed class TargetCandidateSelector : ITargetCandidateSelector
    {
        private readonly ITargetCandidateEvaluator _evaluator;
        private readonly ITargetCandidateComparer _comparer;

        public TargetCandidateSelector(
            ITargetCandidateEvaluator evaluator,
            ITargetCandidateComparer comparer)
        {
            _evaluator = evaluator;
            _comparer = comparer;
        }

        public bool TrySelectBest(
            TargetCandidate[] candidates,
            int count,
            ITargetable currentTarget,
            out TargetCandidate bestCandidate)
        {
            bestCandidate = default;

            if (candidates == null || count <= 0)
                return false;

            var bestScore = float.NegativeInfinity;
            var hasBest = false;

            for (var i = 0; i < count; i++)
            {
                var candidate = candidates[i];
                var score = _evaluator.Evaluate(candidate, currentTarget);

                if (!hasBest ||
                    _comparer.IsBetter(candidate, score, bestCandidate, bestScore, currentTarget))
                {
                    bestCandidate = candidate;
                    bestScore = score;
                    hasBest = true;
                }
            }

            return hasBest;
        }
    }
}