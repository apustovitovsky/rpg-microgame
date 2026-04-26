using System;
using UnityEngine;
using Unity.Cinemachine;

namespace RPG.Gameplay
{
    public sealed class CameraService : ICameraService, ICameraInputHandler, IDisposable
    {
        private readonly CinemachineCamera _virtualCamera;
        private readonly CameraSettingsSO _settings;

        public Transform CurrentTarget => _virtualCamera != null ? _virtualCamera.Follow : null;

        public CameraService(CinemachineCamera camera, CameraSettingsSO settings)
        {
            _virtualCamera = camera;
            _settings = settings;
        }

        public void SetTarget(Transform cameraPivot)
        {
            if (_virtualCamera == null) return;

            _virtualCamera.Follow = cameraPivot;
            _virtualCamera.LookAt = cameraPivot;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void RemoveTarget()
        {
            if (_virtualCamera == null) return;

            _virtualCamera.Follow = null;
            _virtualCamera.LookAt = null;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
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

        public void Dispose()
        {
        }
    }
}
