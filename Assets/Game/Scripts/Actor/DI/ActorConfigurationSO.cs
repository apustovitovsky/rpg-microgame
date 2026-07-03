using Game.CommandSystem;
using Game.Core;
using UnityEngine;
using VContainer;

namespace Game.Actor
{
    [CreateAssetMenu(
        fileName = "ActorConfiguration",
        menuName = "Game/Actor/Actor Configuration")]
    public sealed class ActorConfigurationSO : BuildConfigurationSO
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