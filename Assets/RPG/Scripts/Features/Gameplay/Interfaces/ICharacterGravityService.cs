using UnityEngine;

namespace RPG.Gameplay
{
    public interface ICharacterGravityService
    {
        bool TryJump(CharacterController controller, float jumpHeight);
        Vector3 GetGravityStep(CharacterController controller, float deltaTime);
    }
}
