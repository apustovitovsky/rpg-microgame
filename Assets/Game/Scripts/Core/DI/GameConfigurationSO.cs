using UnityEngine;
using VContainer;

namespace Game.Core
{
    [CreateAssetMenu(
        fileName = "GameConfiguration",
        menuName = "Game/Core/Game Configuration")]
    public sealed class GameConfigurationSO : BuildConfigurationSO
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<GameTimeProvider>(Lifetime.Singleton)
                .AsImplementedInterfaces();
        }
    }
}