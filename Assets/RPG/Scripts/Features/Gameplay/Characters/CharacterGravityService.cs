using UnityEngine;

namespace RPG.Gameplay
{
    public sealed class CharacterGravityService : ICharacterGravityService
    {
        private const float GravityAcceleration = -9.81f;
        private const float GroundedStickVelocity = -2f;

        private float _verticalVelocity;

        public bool TryJump(CharacterController controller, float jumpHeight)
        {
            if (controller == null || !controller.isGrounded)
                return false;

            _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * GravityAcceleration);
            return true;
        }

        public Vector3 GetGravityStep(CharacterController controller, float deltaTime)
        {
            if (controller == null)
                return Vector3.zero;

            if (controller.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = GroundedStickVelocity;

            _verticalVelocity += GravityAcceleration * deltaTime;
            return Vector3.up * (_verticalVelocity * deltaTime);
        }
    }
}
