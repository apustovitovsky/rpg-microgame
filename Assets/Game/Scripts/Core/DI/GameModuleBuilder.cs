using UnityEngine;
using VContainer;

namespace Game.Core
{
    [CreateAssetMenu(
        fileName = "GameModuleBuilder",
        menuName = "Game/Core/Game Module Builder")]
    public sealed class GameModuleBuilder : ModuleBuilder
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<GameTimeProvider>(Lifetime.Singleton)
                .AsImplementedInterfaces();
        }
    }
}