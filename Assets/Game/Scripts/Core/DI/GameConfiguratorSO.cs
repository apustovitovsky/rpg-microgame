using UnityEngine;
using VContainer;

namespace Game.Core
{
    [CreateAssetMenu(
        fileName = "GameConfigurator",
        menuName = "Game/Core/Game Configurator")]
    public sealed class GameConfiguratorSO : BuildConfiguratorSO
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<GameTimeProvider>(Lifetime.Singleton)
                .AsImplementedInterfaces();
        }
    }
}