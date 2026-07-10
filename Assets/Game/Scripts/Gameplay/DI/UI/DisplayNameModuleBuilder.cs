using Game.Core;
using Game.UI;
using UnityEngine;
using VContainer;

namespace Game.Gameplay
{
    [CreateAssetMenu(
        fileName = "DisplayNameModuleBuilder",
        menuName = "Game/Gameplay/Display Name Module Builder")]
    public sealed class DisplayNameModuleBuilder : ModuleBuilder
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<DisplayNameService>(Lifetime.Singleton)
                .AsImplementedInterfaces();
        }
    }
}