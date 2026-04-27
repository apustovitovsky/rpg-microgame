using UnityEngine;
using VContainer.Unity;

namespace Etheria.Gameplay
{
    public interface IPossessionService
    {
        void Possess(LifetimeScope actor);
        void Unpossess();
    }
}
