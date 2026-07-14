using Game.Core;
using UnityEngine;
using VContainer;

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
            builder.Register<CommandReceiver>(Lifetime.Scoped)
                .AsImplementedInterfaces();

            builder.RegisterBinding<ICommandReceiver>();
        }
    }
}