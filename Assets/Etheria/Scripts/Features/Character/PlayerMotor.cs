
using Etheria.Game;
using Etheria.Game.Player;
using UnityEngine;
using VContainer.Unity;

namespace Etheria.Features.Actor
{
    public sealed class PlayerMotor : IActorInputHandler, IActorFacingHandler, ITickable
    {
        private readonly CharacterController _controller;
        private readonly IGameTimeProvider _gameTime;
        private readonly IActorGravityService _gravityService;

        private const float MoveSpeed = 5f;
        private const float JumpHeight = 1.5f;
        private const float JumpBufferDuration = 0.15f;

        private bool _isFirePressed;
        private float _jumpRequestTimeRemaining;
        private Vector3 _faceDirection = Vector3.forward;
        private Vector2 _moveDirection;

        public PlayerMotor(
            CharacterController controller,
            IGameTimeProvider gameTime,
            IActorGravityService gravityService)
        {
            _controller = controller;
            _gameTime = gameTime;
            _gravityService = gravityService;
        }

        public void Tick()
        {
            if (_controller == null) return;

            if (_jumpRequestTimeRemaining > 0f)
                _jumpRequestTimeRemaining -= _gameTime.DeltaTime;

            RotateToFaceDirection();

            Vector3 forward = Vector3.ProjectOnPlane(_faceDirection, Vector3.up);
            if (forward.sqrMagnitude < 0.001f)
                forward = Vector3.ProjectOnPlane(_controller.transform.forward, Vector3.up);

            forward.Normalize();
            Vector3 right = Vector3.Cross(Vector3.up, forward);

            Vector3 move = right * _moveDirection.x + forward * _moveDirection.y;
            if (move.sqrMagnitude > 1f)
                move.Normalize();

            if (_jumpRequestTimeRemaining > 0f &&
                _gravityService.TryJump(_controller, JumpHeight))
            {
                _jumpRequestTimeRemaining = 0f;
            }

            Vector3 horizontalStep = _gameTime.DeltaTime * MoveSpeed * move;
            Vector3 gravityStep = _gravityService.GetGravityStep(_controller, _gameTime.DeltaTime);

            _controller.Move(horizontalStep + gravityStep);
        }

        public void HandleFire(bool isPressed)
        {
            if (isPressed && !_isFirePressed)
                Debug.Log("Player fire");
            _isFirePressed = isPressed;
        }

        public void HandleJump(bool isPressed)
        {
            if (isPressed)
                _jumpRequestTimeRemaining = JumpBufferDuration;
        }

        public void HandleMove(Vector2 vector) => _moveDirection = vector;

        public void HandleFace(Vector3 vector)
        {
            if (vector.sqrMagnitude > 0.001f)
                _faceDirection = vector.normalized;
        }

        private void RotateToFaceDirection()
        {
            Vector3 horizontalFace = Vector3.ProjectOnPlane(_faceDirection, Vector3.up);
            if (horizontalFace.sqrMagnitude < 0.001f) return;
            _controller.transform.rotation = Quaternion.LookRotation(horizontalFace.normalized, Vector3.up);
        }
    }
}
