using Game.Core;
using UnityEngine;
using VContainer;

namespace Game.Player
{
    [DisallowMultipleComponent]
    public sealed class PossessCommandEndpoint :
        MonoBehaviour,
        IModuleInstaller
    {
        public void Install(
            IContainerBuilder builder)
        {
            builder.Register<PossessCommandHandler>(Lifetime.Scoped)
                .AsImplementedInterfaces();
        }
    }
}