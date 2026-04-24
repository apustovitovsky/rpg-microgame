using UnityEngine;

namespace RPG.Gameplay
{
    public interface IPossessionService
    {
        void Possess(GameObject actor);
        void Unpossess();
    }
}
