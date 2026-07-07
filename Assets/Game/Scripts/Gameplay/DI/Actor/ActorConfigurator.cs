using Game.Core;
using UnityEngine;
using VContainer;

namespace Game.Actor
{
    [CreateAssetMenu(
        fileName = "ActorConfigurator",
        menuName = "Game/Gameplay/Actor Configurator")]
    public sealed class ActorConfigurator : BuildConfigurator
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<ActorWorldObjectFactory>(Lifetime.Singleton)
                .AsImplementedInterfaces();
        }
    }
}