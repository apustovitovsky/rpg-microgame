using Etheria.Core.DI;
using UnityEngine;
using VContainer;

namespace Game.Input
{
    [CreateAssetMenu(
        fileName = "InputInstaller",
        menuName = "Game/Input/Input Installer")]
    public sealed class InputInstallerSO : InstallerSO
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<InputActions_Generated>(Lifetime.Singleton);
        }
    }
}