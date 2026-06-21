using Etheria.Core.DI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Etheria.Features.Camera
{
    [CreateAssetMenu(
        fileName = "CameraSystemInstaller",
        menuName = "Etheria/Features/Camera/Camera System Installer")]
    public class CameraFeatureInstallerSO : InstallerSO
    {
        [SerializeField] private CameraSettingsSO _cameraSettings;

        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterComponentInNewPrefab(_cameraSettings.CameraPrefab, Lifetime.Singleton)
                .UnderScopeRoot();
        }
    }
}
