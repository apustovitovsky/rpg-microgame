using Etheria.Core.DI;

using Etheria.Features.Input;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Etheria.Features
{
    [CreateAssetMenu(
        fileName = "InputFeatureInstaller",
        menuName = "Etheria/Features/Input/Input Feature Installer")]
    public class InputFeatureInstallerSO : InstallerSO
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<PlayerInputSource>(Lifetime.Singleton)
                .AsImplementedInterfaces();
        }
    }
}

