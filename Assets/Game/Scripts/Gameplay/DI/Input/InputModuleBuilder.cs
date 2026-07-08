using Game.Core;
using UnityEngine;
using VContainer;

namespace Game.Input
{
    [CreateAssetMenu(
        fileName = "InputModuleBuilder",
        menuName = "Game/Input/Input Module Builder")]
    public sealed class InputModuleBuilder : ModuleBuilder
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<InputActions_Generated>(Lifetime.Singleton);
        }
    }
}