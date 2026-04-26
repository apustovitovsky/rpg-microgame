using System;
using UnityEngine;
using Unity.Cinemachine;
using VContainer.Unity;

namespace RPG.Gameplay
{
    public sealed class CameraService : ICameraService, ICameraInputHandler, ITickable, IDisposable
    {
        private readonly CinemachineCamera _virtualCamera;
        private IActorInputHandler _playerInputHandler;
        private Transform _actorRoot;
        private Transform _lookTarget;
        private float _pitch;
        private float _yaw;

        private const float HorizontalLookSpeed = 0.2f;
        private const float VerticalLookSpeed = -0.15f;
        private const float MinPitch = -35f;
        private const float MaxPitch = 60f;
        private const float MinFieldOfView = 40f;
        private const float MaxFieldOfView = 65f;

        public Transform CurrentTarget => _virtualCamera != null ? _virtualCamera.Follow : null;

        public CameraService(CinemachineCamera camera)
        {
            _virtualCamera = camera;
        }

        public void SetHandler(IActorInputHandler handler)
        {
            _playerInputHandler = handler;
        }

        public void RemoveHandler(IActorInputHandler handler)
        {
            if (_playerInputHandler == handler) _playerInputHandler = null;
        }

        public void SetTarget(Transform actorRoot, Transform lookTarget)
        {
            if (_virtualCamera == null) return;

            _actorRoot = actorRoot;
            _lookTarget = lookTarget;

            _virtualCamera.Follow = lookTarget;
            _virtualCamera.LookAt = null;

            _yaw = _actorRoot != null ? _actorRoot.eulerAngles.y : 0f;
            _pitch = _lookTarget != null ? NormalizeAngle(_lookTarget.localEulerAngles.x) : 0f;
            ApplyLookRotation();
        }

        public void RemoveTarget()
        {
            if (_virtualCamera == null) return;

            _virtualCamera.Follow = null;
            _virtualCamera.LookAt = null;
            _actorRoot = null;
            _lookTarget = null;
        }

        public void HandleLook(Vector2 value)
        {
            if (_actorRoot == null || _lookTarget == null)
                return;

            _yaw += value.x * HorizontalLookSpeed;
            _pitch = Mathf.Clamp(_pitch + value.y * VerticalLookSpeed, MinPitch, MaxPitch);
            ApplyLookRotation();
        }

        public void HandleZoom(float value)
        {
            if (_virtualCamera == null)
                return;

            _virtualCamera.Lens.FieldOfView = Mathf.Clamp(
                _virtualCamera.Lens.FieldOfView + value,
                MinFieldOfView,
                MaxFieldOfView);
        }

        public void Tick()
        {
            if (_lookTarget == null) return;

            Vector3 camForward = _lookTarget.forward;

            if (camForward.sqrMagnitude > 0.001f)
            {
                camForward.Normalize();
                _playerInputHandler?.HandleFace(camForward);
            }
        }

        public void Dispose()
        {
            _playerInputHandler = null;
        }

        private void ApplyLookRotation()
        {
            if (_actorRoot == null || _lookTarget == null)
                return;

            _actorRoot.rotation = Quaternion.Euler(0f, _yaw, 0f);
            _lookTarget.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
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
