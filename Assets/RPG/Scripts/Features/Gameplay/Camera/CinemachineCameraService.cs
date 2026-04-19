using System;
using Unity.Cinemachine;
using UnityEngine;
using VContainer.Unity;

namespace RPG.Gameplay
{
    public sealed class CinemachineCameraService : ICameraService, ICameraInputHandler, ITickable, IDisposable
    {
        private readonly CinemachineCamera _camera;
        private IActorInputHandler _playerInputHandler;

        public CinemachineCameraService(CinemachineCamera camera)
        {
            _camera = camera;
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
            if (_camera == null) return;

            _camera.Follow = target;
            _camera.LookAt = target;
        }

        public void RemoveTarget()
        {
            if (_camera == null) return;

            _camera.Follow = null;
            _camera.LookAt = null;
        }

        public void HandleLook(Vector2 value)
        {
            _camera.transform.Rotate(Vector3.up, value.x);
            _camera.transform.Rotate(Vector3.right, value.y);
        }

        public void HandleZoom(float value)
        {
            _camera.Lens.FieldOfView += value;
        }

        public void Tick()
        {
            if (_camera == null) return;

            Vector3 camForward = _camera.transform.forward;

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
