using System;
using UnityEngine;
using VContainer.Unity;

namespace Etheria.Gameplay
{
    public sealed class PlayerLookService : IPlayerLookService, IPlayerLookInputHandler, ITickable, IDisposable
    {
        private readonly CameraSettingsSO _cameraSettings;
        private IActorInputHandler _actorInputHandler;
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

        public void SetHandler(IActorInputHandler handler)
        {
            _actorInputHandler = handler;
        }

        public void RemoveHandler(IActorInputHandler handler)
        {
            if (_actorInputHandler == handler)
                _actorInputHandler = null;
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
            Vector3 localEulerAngles = _cameraPivot.localEulerAngles;
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

        public void Tick()
        {
            if (_cameraPivot == null || _actorInputHandler == null)
                return;

            Vector3 forward = _cameraPivot.forward;
            if (forward.sqrMagnitude <= 0.001f)
                return;

            _actorInputHandler.HandleFace(forward);
        }

        public void Dispose()
        {
            RemoveTarget();
            _actorInputHandler = null;
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
