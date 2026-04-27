using UnityEngine;

namespace Etheria.Gameplay
{
    public interface IActorGravityService
    {
        bool TryJump(CharacterController controller, float jumpHeight);
        Vector3 GetGravityStep(CharacterController controller, float deltaTime);
    }
}
