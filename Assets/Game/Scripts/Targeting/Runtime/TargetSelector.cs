using System.Collections.Generic;
using UnityEngine;

namespace Game.Targeting
{
    public interface ITargetSelector
    {
        ITargetable SelectBest(
            IReadOnlyCollection<ITargetable> candidates,
            Vector3 origin,
            Vector3 forward);
    }

    public sealed class TargetSelector : ITargetSelector
    {
        private readonly IReadOnlyList<ITargetFilter> _filters;
        private readonly IReadOnlyList<ITargetScorer> _scorers;

        public TargetSelector(
            IReadOnlyList<ITargetFilter> filters,
            IReadOnlyList<ITargetScorer> scorers)
        {
            _filters = filters;
            _scorers = scorers;
        }

        public ITargetable SelectBest(
            IReadOnlyCollection<ITargetable> candidates,
            Vector3 origin,
            Vector3 forward)
        {
            ITargetable best = null;
            float bestScore = float.NegativeInfinity;

            foreach (var candidate in candidates)
            {
                if (!PassesFilters(candidate))
                    continue;

                float score = Score(
                    candidate,
                    origin,
                    forward);

                if (score > bestScore)
                {
                    bestScore = score;
                    best = candidate;
                }
            }

            return best;
        }

        private bool PassesFilters(ITargetable target)
        {
            foreach (var filter in _filters)
            {
                if (!filter.IsMatch(target))
                    return false;
            }

            return true;
        }

        private float Score(
            ITargetable target,
            Vector3 origin,
            Vector3 forward)
        {
            float score = 0f;

            foreach (var scorer in _scorers)
            {
                score += scorer.Score(
                    target,
                    origin,
                    forward);
            }

            return score;
        }
    }
}