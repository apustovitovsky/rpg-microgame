using Game.Core;
using Game.Interaction;
using Game.World;
using UnityEngine;
using VContainer;

namespace Game.Gameplay
{
    [CreateAssetMenu(
        fileName = "InteractionModuleBuilder",
        menuName = "Game/Gameplay/Interaction Module Builder")]
    public sealed class InteractionModuleBuilder : ModuleBuilder
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<InteractionService>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<WorldRegistry<IInteractor>>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<WorldRegistry<IInteractable>>(Lifetime.Singleton)
                .AsImplementedInterfaces();
        }
    }
}