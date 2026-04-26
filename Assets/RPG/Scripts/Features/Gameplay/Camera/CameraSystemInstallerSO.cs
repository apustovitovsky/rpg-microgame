using RPG.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RPG.Gameplay
{
    [CreateAssetMenu(
        fileName = "CameraSystemInstaller",
        menuName = "RPG/Gameplay/Camera/Camera System Installer")]
    public class CameraSystemInstallerSO : InstallerSO
    {
        [SerializeField] private CameraSettingsSO _cameraSettings;

        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_cameraSettings);
            builder.RegisterComponentInNewPrefab(_cameraSettings.CameraPrefab, Lifetime.Singleton);

            builder.RegisterEntryPoint<PlayerLookService>(Lifetime.Singleton)
                .As<IPlayerLookService>()
                .As<IPlayerLookInputHandler>();

            builder.RegisterEntryPoint<CameraService>(Lifetime.Singleton)
                .As<ICameraInputHandler>()
                .As<ICameraService>();

            builder.RegisterBuildCallback(container =>
            {
                var lookService = container.Resolve<IPlayerLookInputHandler>();
                var cameraService = container.Resolve<ICameraInputHandler>();
                var inputService = container.Resolve<IGameplayInputRouter>();

                inputService.SetHandler(lookService);
                inputService.SetHandler(cameraService);
            });
        }
    }
}
