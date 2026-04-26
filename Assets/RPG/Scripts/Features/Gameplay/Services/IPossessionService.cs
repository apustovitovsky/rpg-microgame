using UnityEngine;
using VContainer.Unity;

namespace RPG.Gameplay
{
    public interface IPossessionService
    {
        void Possess(LifetimeScope actor);
        void Unpossess();
    }
}
