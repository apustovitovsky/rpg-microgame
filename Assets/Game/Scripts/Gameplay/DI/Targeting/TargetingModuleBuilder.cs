using Game.Core;
using Game.Targeting;
using UnityEngine;
using VContainer;

namespace Game.Gameplay
{
    [CreateAssetMenu(
        fileName = "TargetingModuleBuilder",
        menuName = "Game/Gameplay/Targeting Module Builder")]
    public sealed class TargetingModuleBuilder :
        ModuleBuilder
    {
        public override void Install(
            IContainerBuilder builder)
        {
            builder.Register<Registry<ITargetable>>(
                    Lifetime.Singleton)
                .AsImplementedInterfaces();
        }
    }
}