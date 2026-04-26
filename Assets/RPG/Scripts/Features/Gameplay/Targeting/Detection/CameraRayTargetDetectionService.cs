using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Gameplay
{
    public sealed class CameraRayTargetDetectionService : ITargetDetectionService
    {
        private readonly Camera _camera;
        private readonly ICameraService _cameraService;
        private readonly ColliderTargetResolver _targetResolver;
        private readonly TargetingSettingsSO _settings;

        public CameraRayTargetDetectionService(
            Camera camera,
            ICameraService cameraService,
            ColliderTargetResolver targetResolver,
            TargetingSettingsSO settings)
        {
            _camera = camera;
            _cameraService = cameraService;
            _targetResolver = targetResolver;
            _settings = settings;
        }

        public IReadOnlyList<ITargetable> GetCandidates()
        {
            if (_camera == null)
                return Array.Empty<ITargetable>();

            Debug.DrawRay(
                _camera.transform.position,
                _camera.transform.forward * _settings.MaxDistance,
                Color.red,
                1f);

            var ray = new Ray(_camera.transform.position, _camera.transform.forward);
            if (!Physics.Raycast(ray, out var hit, _settings.MaxDistance))
                return Array.Empty<ITargetable>();

            if (!_targetResolver.TryResolve(hit, out var targetable))
                return Array.Empty<ITargetable>();

            return new[] { targetable };
        }
    }
}
