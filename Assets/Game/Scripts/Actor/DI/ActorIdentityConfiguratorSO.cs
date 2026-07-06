using Game.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Actor
{
    [CreateAssetMenu(
        fileName = "ActorIdentityConfigurator",
        menuName = "Game/Actor/Actor Identity Configurator")]
    public sealed class ActorIdentityConfiguratorSO : BuildConfiguratorSO
    {
        public override void Install(
            IContainerBuilder builder)
        {
            builder.Register<ActorIdentity>(Lifetime.Scoped)
                .AsImplementedInterfaces();

            builder.RegisterComponentInScope<ActorView>()
                .AsImplementedInterfaces();

            builder.RegisterComponentInScope<ActorLookController>();

            builder.RegisterComponentInScope<MovementController>();

            builder.RegisterComponentInScope<TargetingController>()
                .AsSelf()
                .AsImplementedInterfaces();

            builder.RegisterComponentInScope<ActorTargetable>()
                .AsSelf()
                .AsImplementedInterfaces();
        }
    }
}