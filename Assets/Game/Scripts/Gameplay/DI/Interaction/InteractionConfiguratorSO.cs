using Game.Core;
using Game.Interaction;
using UnityEngine;
using VContainer;

namespace Game.Gameplay
{
    [CreateAssetMenu(
        fileName = "InteractionConfigurator",
        menuName = "Game/Gameplay/Interaction Configurator")]
    public sealed class InteractionConfiguratorSO : BuildConfiguratorSO
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<InteractionService>(Lifetime.Singleton)
                .AsImplementedInterfaces();
        }
    }
}