using Game.CommandSystem;
using Game.Core;
using UnityEngine;
using VContainer;

namespace Game.Gameplay
{
    [CreateAssetMenu(
        fileName = "CommandSystemModuleBuilder",
        menuName = "Game/Gameplay/Command System Module Builder")]
    public sealed class CommandSystemModuleBuilder :
        ModuleBuilder
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<InstanceIndex<ICommandReceiver>>(
                    Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<CommandManager>(Lifetime.Singleton)
                .AsImplementedInterfaces();
        }
    }
}