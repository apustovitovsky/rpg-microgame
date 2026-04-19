using UnityEngine;

namespace RPG.Gameplay
{
    public interface ICameraInputHandler
    {
        void HandleLook(Vector2 value);
        void HandleZoom(float value);
    }
}
