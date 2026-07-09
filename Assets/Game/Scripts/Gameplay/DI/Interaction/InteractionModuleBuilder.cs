using Game.Core;
using Game.Interaction;
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
        }
    }
}