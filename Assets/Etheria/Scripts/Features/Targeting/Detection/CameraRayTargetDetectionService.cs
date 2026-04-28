using System;
using System.Collections.Generic;
using Etheria.Features.Camera;
using Etheria.Game.Camera;
using UnityEngine;

namespace Etheria.Features.Targeting
{
    public sealed class CameraRayTargetDetectionService : ITargetDetectionService
    {
        private readonly ICameraRayProvider _cameraRayProvider;
        private readonly ColliderTargetResolver _targetResolver;
        private readonly TargetingSettingsSO _settings;
        private readonly IPlayerLookService _playerLookService;



        public CameraRayTargetDetectionService(
            ICameraRayProvider cameraRayProvider,
            IPlayerLookService playerLookService,
            ColliderTargetResolver targetResolver,
            TargetingSettingsSO settings)
        {
            _cameraRayProvider = cameraRayProvider;
            _playerLookService = playerLookService;
            _targetResolver = targetResolver;
            _settings = settings;
        }

        public IReadOnlyList<ITargetable> GetCandidates()
        {
            var ray = _cameraRayProvider.GetForwardRay();

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

                var selfActorRoot = _playerLookService.CurrentActorRoot;
                if (selfActorRoot != null &&
                    targetable.AimPoint != null &&
                    targetable.AimPoint.IsChildOf(selfActorRoot))
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
