using UnityEngine;

namespace Etheria.Features
{
    public interface IActorInputHandler
    {
        void HandleMove(Vector2 vector);
        void HandleJump(bool isPressed);
        void HandleFire(bool isPressed);
        void HandleFace(Vector3 vector);
    }
}

