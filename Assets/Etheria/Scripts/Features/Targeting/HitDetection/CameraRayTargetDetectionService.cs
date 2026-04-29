using System;
using System.Collections.Generic;
using Etheria.Game.Player;
using Etheria.Game.Targeting;
using UnityEngine;

namespace Etheria.Features.Targeting
{
    public sealed class CameraRayTargetDetectionService : ITargetDetectionService
    {
        private readonly IViewRayProvider _viewRayProvider;
        private readonly IControlledTargetProvider _selfProvider;
        private readonly ColliderTargetResolver _targetResolver;
        private readonly TargetingSettingsSO _settings;

        public CameraRayTargetDetectionService(
            IViewRayProvider viewRayProvider,
            IControlledTargetProvider selfProvider,
            ColliderTargetResolver targetResolver,
            TargetingSettingsSO settings)
        {
            _viewRayProvider = viewRayProvider;
            _selfProvider = selfProvider;
            _targetResolver = targetResolver;
            _settings = settings;
        }

        public IReadOnlyList<ITargetable> GetCandidates()
        {
            var ray = _viewRayProvider.GetRay();

            var hits = Physics.SphereCastAll(
                ray,
                5.75f,
                _settings.MaxDistance,
                _settings.TargetingMask);

            if (hits == null || hits.Length == 0)
                return Array.Empty<ITargetable>();

            Array.Sort(hits, static (left, right) => left.distance.CompareTo(right.distance));

            var candidates = new List<ITargetable>(hits.Length);

            for (var i = 0; i < hits.Length; i++)
            {
                var hit = hits[i];
                if (!_targetResolver.TryResolve(hit, out var targetable))
                    continue;

                if (ReferenceEquals(targetable, _selfProvider.ControlledTarget))
                {
                    continue;
                }

                if (candidates.Contains(targetable))
                    continue;

                candidates.Add(targetable);
            }

            return candidates;
        }
    }
}
