using Etheria.Core.DI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Etheria.Game.Camera
{
    [CreateAssetMenu(
        fileName = "GameCameraInstaller",
        menuName = "Etheria/Game/Camera/Game Camera Installer")]
    public class GameCameraInstallerSO : InstallerSO
    {
        [SerializeField] private GameCameraRig _mainCamera;

        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterComponentInNewPrefab(_mainCamera, Lifetime.Singleton)
                .UnderScopeRoot()
                .As<ICameraTransformProvider>();

            builder.RegisterBuildCallback(container =>
            {
                container.Resolve<ICameraTransformProvider>();
            });
        }
    }
}
