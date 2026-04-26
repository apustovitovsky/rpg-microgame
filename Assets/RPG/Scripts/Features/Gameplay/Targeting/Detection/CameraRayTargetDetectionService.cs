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

        public IReadOnlyList<IActorTargetable> GetCandidates()
        {
            if (_camera == null)
            {
                Debug.LogWarning("Targeting raycast skipped: no camera was resolved.");
                return Array.Empty<IActorTargetable>();
            }

            var origin = _camera.transform.position;
            var direction = _camera.transform.forward;
            var endPoint = origin + direction * _settings.MaxDistance;

            Debug.DrawLine(origin, endPoint, Color.red, 2f, false);
            Debug.Log($"Targeting raycast: camera={_camera.name}, origin={origin}, direction={direction}, end={endPoint}, maxDistance={_settings.MaxDistance}");

            var ray = new Ray(origin, direction);
            var hits = Physics.RaycastAll(ray, _settings.MaxDistance);
            if (hits.Length == 0)
            {
                Debug.Log("Targeting raycast missed all colliders.");
                return Array.Empty<IActorTargetable>();
            }

            Array.Sort(hits, static (left, right) => left.distance.CompareTo(right.distance));

            var ownerRoot = _cameraService.CurrentTarget != null
                ? _cameraService.CurrentTarget.root
                : null;

            for (var i = 0; i < hits.Length; i++)
            {
                var hit = hits[i];
                if (ownerRoot != null && hit.collider.transform.root == ownerRoot)
                {
                    Debug.Log($"Targeting raycast ignored self collider={hit.collider.name}, distance={hit.distance}");
                    continue;
                }

                Debug.DrawLine(origin, hit.point, Color.green, 2f, false);
                Debug.Log($"Targeting raycast hit collider={hit.collider.name}, point={hit.point}, distance={hit.distance}");

                if (!_targetResolver.TryResolve(hit, out var targetable))
                {
                    Debug.Log($"Targeting raycast hit '{hit.collider.name}', but it does not resolve to {nameof(ActorHitbox)}.");
                    return Array.Empty<IActorTargetable>();
                }

                Debug.Log($"Targeting resolved actor target: {targetable.DisplayName}");
                return new[] { targetable };
            }

            Debug.Log("Targeting raycast only hit the possessed actor.");
            return Array.Empty<IActorTargetable>();
        }
    }
}
