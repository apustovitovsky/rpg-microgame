using UnityEngine;

namespace RPG.Gameplay
{
    public interface IPlayerInputHandler
    {
        void HandleMove(Vector2 vector);
        void HandleJump(bool isPressed);
        void HandleFire(bool isPressed);
        void HandleFace(Vector3 vector);
    }
}
