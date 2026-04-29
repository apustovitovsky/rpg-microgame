using System;
using Etheria.Game.Targeting;
using UnityEngine;

namespace Etheria.Features.Targeting
{
    public interface ITargetCandidateProvider
    {
        int GetCandidates(
            TargetCandidate[] buffer,
            ITargetable ignoredTarget = null);
    }

    public sealed class TargetCandidateProvider : ITargetCandidateProvider
    {
        private readonly ITargetHitProvider _physicsQuery;
        private readonly ITargetCandidateResolver _resolver;
        private readonly ITargetCandidateFilter _filter;
        private readonly ITargetLineOfSightChecker _visibilityChecker;

        private readonly RaycastHit[] _hits;

        public TargetCandidateProvider(
            ITargetHitProvider physicsQuery,
            ITargetCandidateResolver resolver,
            ITargetCandidateFilter filter,
            TargetingSettingsSO settings,
            ITargetLineOfSightChecker visibilityChecker)
        {
            _physicsQuery = physicsQuery;
            _resolver = resolver;
            _filter = filter;
            _visibilityChecker = visibilityChecker;

            _hits = new RaycastHit[settings.MaxTargetCandidates];
        }

        public int GetCandidates(
            TargetCandidate[] buffer,
            ITargetable ignoredTarget = null)
        {
            if (buffer == null || buffer.Length == 0)
                return 0;

            var hitCount = _physicsQuery.GetHits(_hits);

            if (hitCount <= 0)
                return 0;

            var resultCount = 0;

            for (var i = 0; i < hitCount; i++)
            {
                var hit = _hits[i];

                if (!_resolver.TryResolve(hit, out var candidate))
                    continue;

                if (!_filter.IsAllowed(candidate, ignoredTarget))
                    continue;

                if (!_visibilityChecker.HasLineOfSight(candidate))
                    continue;

                if (Contains(buffer, resultCount, candidate.Targetable))
                    continue;

                buffer[resultCount++] = candidate;

                if (resultCount >= buffer.Length)
                    break;
            }

            return resultCount;
        }

        private static bool Contains(
            TargetCandidate[] buffer,
            int count,
            ITargetable targetable)
        {
            for (var i = 0; i < count; i++)
            {
                if (ReferenceEquals(buffer[i].Targetable, targetable))
                    return true;
            }

            return false;
        }
    }
}