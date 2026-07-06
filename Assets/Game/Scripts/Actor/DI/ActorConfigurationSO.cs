using Game.Core;
using UnityEngine;
using VContainer;

namespace Game.Actor
{
    [CreateAssetMenu(
        fileName = "ActorConfigurator",
        menuName = "Game/Actor/Actor Configurator")]
    public sealed class ActorConfiguratorSO : BuildConfiguratorSO
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<ActorSpawner>(Lifetime.Singleton)
                .AsImplementedInterfaces();
        }
    }
}