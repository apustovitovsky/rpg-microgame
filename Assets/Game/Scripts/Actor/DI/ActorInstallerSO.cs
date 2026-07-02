using Etheria.Core.DI;
using Game.CommandSystem;
using UnityEngine;
using VContainer;

namespace Game.Actor
{
    [CreateAssetMenu(
        fileName = "ActorInstaller",
        menuName = "Game/Actor/Actor Installer")]
    public sealed class ActorInstallerSO : InstallerSO
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<ActorRegistry>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<ActionGate>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<CommandService>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<StartDialogueCommandHandler>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<AttackCommandHandler>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<MoveToLocationCommandHandler>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<ActorSpawner>(Lifetime.Singleton)
                .AsImplementedInterfaces();
        }
    }
}