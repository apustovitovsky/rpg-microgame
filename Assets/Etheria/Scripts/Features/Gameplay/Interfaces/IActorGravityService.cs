using UnityEngine;

namespace Etheria.Features.Gameplay
{
    public interface IActorGravityService
    {
        bool TryJump(CharacterController controller, float jumpHeight);
        Vector3 GetGravityStep(CharacterController controller, float deltaTime);
    }
}

