using Game.Core;
using Game.Interaction;
using Game.World;
using UnityEngine;
using VContainer;

namespace Game.Gameplay
{
    [CreateAssetMenu(
        fileName = "InteractionConfigurator",
        menuName = "Game/Gameplay/Interaction Configurator")]
    public sealed class InteractionConfiguratorSO : BuildConfigurator
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<InteractionService>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<WorldRegistry<IInteractable>>(Lifetime.Singleton)
                .AsImplementedInterfaces();
        }
    }
}