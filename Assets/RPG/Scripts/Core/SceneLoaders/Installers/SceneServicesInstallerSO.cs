
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RPG.Core
{
    [CreateAssetMenu(fileName = "SceneServicesInstaller", menuName = "RPG/Core/SceneServicesInstaller")]
    public class SceneServicesInstallerSO : InstallerSO
    {
        public override void Install(in InstallContext context)
        {
            var builder = context.Builder;

            builder.RegisterEntryPoint<SceneNavigator>()
                .As<ISessionNavigator>();

            builder.Register<SceneStackLoader>(Lifetime.Singleton);
        }
    }
}

