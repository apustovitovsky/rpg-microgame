using Etheria.Core.DI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Etheria.Features.Gameplay
{
    [CreateAssetMenu(
        fileName = "CameraSystemInstaller",
        menuName = "Etheria/Gameplay/Camera/Camera System Installer")]
    public class CameraSystemInstallerSO : ScopeInstallerSO
    {
        [SerializeField] private CameraSettingsSO _cameraSettings;

        public override void Install(IContainerBuilder builder, GameObject rootObject)
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

