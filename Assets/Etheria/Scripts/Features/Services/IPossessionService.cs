using UnityEngine;
using VContainer.Unity;

namespace Etheria.Features
{
    public interface IPossessionService
    {
        void Possess(LifetimeScope actor);
        void Unpossess();
    }
}

