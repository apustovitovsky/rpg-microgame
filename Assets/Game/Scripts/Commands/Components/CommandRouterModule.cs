using Game.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Commands
{
    [DisallowMultipleComponent]
    public sealed class CommandRouterModule :
        MonoBehaviour,
        IModuleInstaller
    {
        public void Install(
            IContainerBuilder builder)
        {
            builder.Register<CommandRouter>(Lifetime.Scoped)
                .AsImplementedInterfaces();

            builder.RegisterEntryPoint<CommandRouterBinding>(
                Lifetime.Scoped);
        }
    }
}