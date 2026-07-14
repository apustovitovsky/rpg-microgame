using Game.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Commands
{
    [DisallowMultipleComponent]
    public sealed class CommandReceiverEndpoint :
        MonoBehaviour,
        IPrefabInstaller
    {
        public void Install(
            IContainerBuilder builder)
        {
            builder.Register<WorldCommandReceiver>(Lifetime.Scoped)
                .AsImplementedInterfaces();

            builder.RegisterEntryPoint<
                RegistryBinding<ICommandReceiver>>(
                Lifetime.Scoped);
        }
    }
}