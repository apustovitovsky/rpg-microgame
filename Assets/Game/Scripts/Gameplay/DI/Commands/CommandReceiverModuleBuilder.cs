using Game.Commands;
using Game.Core;
using Game.Interaction;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Gameplay
{
    [CreateAssetMenu(
        fileName = "CommandReceiverModuleBuilder",
        menuName = "Game/Gameplay/Command Receiver Module Builder")]
    public sealed class CommandReceiverModuleBuilder :
        ModuleBuilder
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<InteractCommandHandler>(Lifetime.Scoped)
                .AsImplementedInterfaces();

            builder.Register<WorldCommandReceiver>(Lifetime.Scoped)
                .AsImplementedInterfaces();

            builder.RegisterEntryPoint<
                RegistryBinding<ICommandReceiver>>(
                Lifetime.Scoped);
        }
    }
}