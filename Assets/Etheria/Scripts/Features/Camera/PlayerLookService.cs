using Etheria.Game.Camera;
using UnityEngine;

namespace Etheria.Features.Camera
{
    public sealed class PlayerLookService : IPlayerLookService, IPlayerLookInputHandler
    {
        private readonly CameraSettingsSO _cameraSettings;
        private Transform _actorRoot;
        private Transform _cameraPivot;
        private float _pitch;
        private float _yaw;

        public Transform CurrentPivot => _cameraPivot;

        public Vector3 Forward => _cameraPivot != null ? _cameraPivot.forward : Vector3.forward;

        public PlayerLookService(CameraSettingsSO cameraSettings)
        {
            _cameraSettings = cameraSettings;
        }

        public void SetTarget(Transform actorRoot, Transform cameraPivot)
        {
            _actorRoot = actorRoot;
            _cameraPivot = cameraPivot;

            if (_cameraPivot == null)
            {
                _actorRoot = null;
                _yaw = 0f;
                _pitch = 0f;
                return;
            }

            _yaw = _actorRoot != null ? NormalizeAngle(_actorRoot.eulerAngles.y) : 0f;
            var localEulerAngles = _cameraPivot.localEulerAngles;
            _pitch = NormalizeAngle(localEulerAngles.x);
            ApplyLookRotation();
        }

        public void RemoveTarget()
        {
            _actorRoot = null;
            _cameraPivot = null;
            _yaw = 0f;
            _pitch = 0f;
        }

        public void HandleLook(Vector2 value)
        {
            if (_cameraPivot == null || _cameraSettings == null)
                return;

            _yaw += value.x * _cameraSettings.HorizontalLookSensitivity;
            _pitch = Mathf.Clamp(
                _pitch + value.y * _cameraSettings.VerticalLookSensitivity,
                _cameraSettings.MinPitch,
                _cameraSettings.MaxPitch);

            ApplyLookRotation();
        }

        private void ApplyLookRotation()
        {
            if (_actorRoot == null || _cameraPivot == null)
                return;

            if (_actorRoot == _cameraPivot)
            {
                _cameraPivot.localRotation = Quaternion.Euler(_pitch, _yaw, 0f);
                return;
            }

            _actorRoot.rotation = Quaternion.Euler(0f, _yaw, 0f);
            _cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        private static float NormalizeAngle(float angle)
        {
            while (angle > 180f)
                angle -= 360f;

            while (angle < -180f)
                angle += 360f;

            return angle;
        }
    }
}
