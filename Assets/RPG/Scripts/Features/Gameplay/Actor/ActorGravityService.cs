using UnityEngine;

namespace RPG.Gameplay
{
    public sealed class ActorGravityService : IActorGravityService
    {
        private const float GroundedStickVelocity = -2f;
        private readonly float _gravityForce;
        private float _verticalVelocity;

        public ActorGravityService(float gravityForce)
        {
            _gravityForce = gravityForce;
        }

        public bool TryJump(CharacterController controller, float jumpHeight)
        {
            if (controller == null || !controller.isGrounded)
                return false;

            _verticalVelocity = Mathf.Sqrt(jumpHeight * -5f * _gravityForce);
            return true;
        }

        public Vector3 GetGravityStep(CharacterController controller, float deltaTime)
        {
            if (controller == null)
                return Vector3.zero;

            if (controller.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = GroundedStickVelocity;

            _verticalVelocity += _gravityForce * deltaTime;
            return Vector3.up * (_verticalVelocity * deltaTime);
        }
    }
}
