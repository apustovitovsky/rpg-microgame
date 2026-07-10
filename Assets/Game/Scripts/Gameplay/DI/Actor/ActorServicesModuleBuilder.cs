using Game.Core;
using UnityEngine;
using VContainer;

namespace Game.Actor
{
    [CreateAssetMenu(
        fileName = "ActorModuleBuilder",
        menuName = "Game/Gameplay/Actor Module Builder")]
    public sealed class ActorServiceModuleBuilder : ModuleBuilder
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<ActorFactory>(Lifetime.Singleton);

            builder.Register<ActorSpawner>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<ActorService>(Lifetime.Singleton)
                .AsImplementedInterfaces();
        }
    }
}