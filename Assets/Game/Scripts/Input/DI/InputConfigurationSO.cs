using Game.Core;
using UnityEngine;
using VContainer;

namespace Game.Input
{
    [CreateAssetMenu(
        fileName = "InputConfiguration",
        menuName = "Game/Input/Input Configuration")]
    public sealed class InputConfigurationSO : BuildConfigurator
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<InputActions_Generated>(Lifetime.Singleton);
        }
    }
}