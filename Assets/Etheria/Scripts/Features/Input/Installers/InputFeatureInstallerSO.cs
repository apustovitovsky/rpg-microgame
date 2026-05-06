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
    public class InputFeatureInstallerSO : ScopeInstallerSO
    {
        public override void Install(IContainerBuilder builder, GameObject rootObject)
        {
            // builder.RegisterEntryPoint<PlayerAvatarInputBinder>(Lifetime.Singleton);

            builder.Register<GameInputRouter>(Lifetime.Singleton)
                .AsImplementedInterfaces();
        }
    }
}

