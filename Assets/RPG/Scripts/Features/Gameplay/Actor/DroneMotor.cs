using RPG.Core;
using UnityEngine;
using VContainer.Unity;

namespace RPG.Gameplay
{
    public sealed class DroneMotor : IActorInputHandler, ITickable
    {
        private readonly CharacterController _controller;
        private readonly ITimeProvider _gameTime;
        private const float MoveSpeed = 5f;
        private Vector2 _moveDirection;
        private Vector3 _faceDirection = Vector3.forward;
        private bool _isJumpPressed;
        private bool _isFirePressed;

        public DroneMotor(CharacterController controller, ITimeProvider gameTime)
        {
            _controller = controller;
            _gameTime = gameTime;
        }

        public void Tick()
        {
            RotateToFaceDirection();

            Vector3 forward = _faceDirection.sqrMagnitude > 0.001f ? _faceDirection : _controller.transform.forward;
            forward.Normalize();
            Vector3 right = Vector3.Cross(Vector3.up, forward);

            if (right.sqrMagnitude < 0.001f)
                right = _controller.transform.right;

            right.Normalize();

            Vector3 move = right * _moveDirection.x + forward * _moveDirection.y;

            if (_isJumpPressed)
                move += Vector3.up;

            if (move.sqrMagnitude > 1f)
                move.Normalize();

            Vector3 step = _gameTime.DeltaTime * MoveSpeed * move;
            if (_controller != null)
                _controller.Move(step);
            else
                _controller.transform.position += step;
        }

        public void HandleFire(bool isPressed)
        {
            if (isPressed && !_isFirePressed)
                Debug.Log("Drone fire");
            _isFirePressed = isPressed;
        }

        public void HandleJump(bool isPressed) => _isJumpPressed = isPressed;
        public void HandleMove(Vector2 value) => _moveDirection = value;

        public void HandleFace(Vector3 value)
        {
            if (value.sqrMagnitude > 0.001f)
                _faceDirection = value.normalized;
        }

        private void RotateToFaceDirection()
        {
            Vector3 horizontalFace = Vector3.ProjectOnPlane(_faceDirection, Vector3.up);
            if (horizontalFace.sqrMagnitude < 0.001f) return;
            _controller.transform.rotation = Quaternion.LookRotation(horizontalFace.normalized, Vector3.up);
        }
    }
}
