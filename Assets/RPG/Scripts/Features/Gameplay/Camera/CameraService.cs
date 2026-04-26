using System;
using Unity.Cinemachine;
using UnityEngine;
using VContainer.Unity;

namespace RPG.Gameplay
{
    public sealed class CameraService : ICameraService, ICameraInputHandler, ITickable, IDisposable
    {
        private readonly CinemachineCamera _mainCamera;
        private IActorInputHandler _playerInputHandler;

        public Transform CurrentTarget => _mainCamera != null ? _mainCamera.Follow : null;

        public CameraService(CinemachineCamera camera)
        {
            _mainCamera = camera;
        }

        public void SetHandler(IActorInputHandler handler)
        {
            _playerInputHandler = handler;
        }

        public void RemoveHandler(IActorInputHandler handler)
        {
            if (_playerInputHandler == handler) _playerInputHandler = null;
        }

        public void SetTarget(Transform target)
        {
            if (_mainCamera == null) return;

            _mainCamera.Follow = target;
            _mainCamera.LookAt = target;
        }

        public void RemoveTarget()
        {
            if (_mainCamera == null) return;

            _mainCamera.Follow = null;
            _mainCamera.LookAt = null;
        }

        public void HandleLook(Vector2 value)
        {
            Debug.Log($"HandleLook: {value}"); // Works
            _mainCamera.transform.Rotate(Vector3.up, value.x);
            _mainCamera.transform.Rotate(Vector3.right, value.y);
        }

        public void HandleZoom(float value)
        {
            _mainCamera.Lens.FieldOfView += value;
        }

        public void Tick()
        {
            if (_mainCamera == null) return;

            Vector3 camForward = _mainCamera.transform.forward;

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
    }
}
