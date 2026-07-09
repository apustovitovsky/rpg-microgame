using Game.Core;
using Game.Targeting;
using Game.World;
using UnityEngine;
using VContainer;

namespace Game.Actor
{
    [CreateAssetMenu(
        fileName = "ActorModuleBuilder",
        menuName = "Game/Gameplay/Actor Module Builder")]
    public sealed class ActorModuleBuilder : ModuleBuilder
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<WorldRegistry<IWorldActor>>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<WorldRegistry<ITargetProvider>>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<ActorWorldObjectFactory>(Lifetime.Singleton);

            builder.Register<ActorSpawner>(Lifetime.Singleton)
                .AsImplementedInterfaces();
        }
    }
}