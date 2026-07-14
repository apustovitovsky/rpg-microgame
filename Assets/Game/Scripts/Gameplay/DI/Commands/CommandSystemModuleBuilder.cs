using Game.Commands;
using Game.Core;
using UnityEngine;
using VContainer;

namespace Game.Gameplay
{
    [CreateAssetMenu(
        fileName = "CommandsModuleBuilder",
        menuName = "Game/Gameplay/Command System Module Builder")]
    public sealed class CommandsModuleBuilder :
        ModuleBuilder
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<Registry<ICommandReceiver>>(
                    Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<CommandManager>(Lifetime.Singleton)
                .AsImplementedInterfaces();
        }
    }
}