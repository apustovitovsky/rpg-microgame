using System;
using System.Collections.Generic;
using Etheria.Game.Camera;
using UnityEngine;

namespace Etheria.Features.Targeting
{
    public sealed class CameraRayTargetDetectionService : ITargetDetectionService
    {
        private readonly ICameraRayProvider _cameraRayProvider;
        private readonly ColliderTargetResolver _targetResolver;
        private readonly TargetingSettingsSO _settings;

        public CameraRayTargetDetectionService(
            ICameraRayProvider cameraRayProvider,
            ColliderTargetResolver targetResolver,
            TargetingSettingsSO settings)
        {
            _cameraRayProvider = cameraRayProvider;
            _targetResolver = targetResolver;
            _settings = settings;
        }

        public IReadOnlyList<ITargetable> GetCandidates()
        {
            var ray = _cameraRayProvider.GetForwardRay();
            if (!Physics.Raycast(ray, out var hit, _settings.MaxDistance))
                return Array.Empty<ITargetable>();

            if (!_targetResolver.TryResolve(hit, out var targetable))
                return Array.Empty<ITargetable>();

            return new[] { targetable };
        }
    }
}

