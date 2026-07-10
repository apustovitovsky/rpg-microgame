using Game.CommandSystem;
using Game.Core;
using Game.Interaction;
using UnityEngine;
using VContainer;

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
        }
    }
}