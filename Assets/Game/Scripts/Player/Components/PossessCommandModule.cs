using Game.Commands;
using Game.Core;
using UnityEngine;
using VContainer;

namespace Game.Player
{
    [DisallowMultipleComponent]
    public sealed class PossessCommandModule :
        MonoBehaviour,
        IModuleInstaller
    {
        public void Install(
            IContainerBuilder builder)
        {
            builder.RegisterCommandRoutes<
                PossessionRoutes>();

            builder.RegisterCommandRoute<
                PossessionRoutes,
                PossessCommand,
                PossessionResult>();
        }
    }
}