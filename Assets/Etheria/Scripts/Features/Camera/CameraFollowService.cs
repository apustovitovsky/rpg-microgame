using System;
using UnityEngine;
using Unity.Cinemachine;
using Etheria.Game.Camera;

namespace Etheria.Features.Camera
{
    public sealed class CameraFollowService : ICameraFollowService, ICameraInputHandler, IDisposable
    {
        private readonly CinemachineCamera _virtualCamera;
        private readonly CameraSettingsSO _settings;

        public Transform CurrentFollowTarget => _virtualCamera != null ? _virtualCamera.Follow : null;

        public CameraFollowService(CinemachineCamera camera, CameraSettingsSO settings)
        {
            _virtualCamera = camera;
            _settings = settings;
        }

        public void SetTarget(Transform target)
        {
            if (_virtualCamera == null) return;

            _virtualCamera.Follow = target;
            _virtualCamera.LookAt = target;

            SetCursorLocked(true);
        }

        public void RemoveTarget()
        {
            if (_virtualCamera == null) return;

            _virtualCamera.Follow = null;
            _virtualCamera.LookAt = null;

            SetCursorLocked(false);
        }

        public void HandleZoom(float value)
        {
            if (_virtualCamera == null || _settings == null)
                return;

            _virtualCamera.Lens.FieldOfView = Mathf.Clamp(
                _virtualCamera.Lens.FieldOfView + value,
                _settings.MinFieldOfView,
                _settings.MaxFieldOfView);
        }


        public void SetCursorLocked(bool isLocked)
        {
            Cursor.lockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !isLocked;
        }

        public void Dispose()
        {
        }
    }
}

