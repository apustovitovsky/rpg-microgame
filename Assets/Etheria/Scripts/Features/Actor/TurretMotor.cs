
using Etheria.Game;
using Etheria.Game.Player;
using UnityEngine;
using VContainer.Unity;

namespace Etheria.Features.Actor
{
    public sealed class TurretMotor : IActorInputHandler, ITickable
    {
        private readonly CharacterController _controller;
        private readonly IGameTimeProvider _gameTime;
        private readonly IActorGravityService _gravityService;
        private Vector3 _faceDirection = Vector3.forward;
        private bool _isFirePressed;

        public TurretMotor(
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

            RotateToFaceDirection();
            _controller.Move(_gravityService.GetGravityStep(_controller, _gameTime.DeltaTime));
        }

        public void HandleFire(bool isPressed)
        {
            if (isPressed && !_isFirePressed)
                Debug.Log("Turret fire");
            _isFirePressed = isPressed;
        }

        public void HandleJump(bool isPressed) { }
        public void HandleMove(Vector2 value) { }

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

