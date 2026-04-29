using Etheria.Core.DI;
using Etheria.Game.Camera;
using Etheria.Game.Input;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Etheria.Features.Camera
{
    [CreateAssetMenu(
        fileName = "CameraSystemInstaller",
        menuName = "Etheria/Features/Camera/Camera System Installer")]
    public class CameraFeatureInstallerSO : ScopeInstallerSO
    {
        [SerializeField] private CameraSettingsSO _cameraSettings;

        public override void Install(IContainerBuilder builder, GameObject rootObject)
        {
            builder.RegisterInstance(_cameraSettings);
            builder.RegisterComponentInNewPrefab(_cameraSettings.CameraPrefab, Lifetime.Singleton);

            builder.RegisterEntryPoint<PlayerAvatarCameraBinder>(Lifetime.Singleton);

            builder.Register<PlayerLookService>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.RegisterEntryPoint<CameraService>(Lifetime.Singleton)
                .As<ICameraInputHandler>()
                .As<ICameraService>();

            builder.RegisterBuildCallback(container =>
            {
                var lookService = container.Resolve<IPlayerLookInputHandler>();
                var cameraService = container.Resolve<ICameraInputHandler>();
                var inputService = container.Resolve<IGameInputRouter>();

                inputService.SetHandler(lookService);
                inputService.SetHandler(cameraService);
            });
        }
    }
}

