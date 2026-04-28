using Etheria.Core.DI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Etheria.Game.Camera
{
    [CreateAssetMenu(
        fileName = "GameCameraInstaller",
        menuName = "Etheria/Game/Camera/Game Camera Installer")]
    public class GameCameraInstallerSO : ScopeInstallerSO
    {
        [SerializeField] private GameCameraRig _mainCamera;

        public override void Install(IContainerBuilder builder, GameObject rootObject)
        {
            builder.RegisterComponentInNewPrefab(_mainCamera, Lifetime.Singleton)
                .UnderTransform(rootObject.transform)
                .As<IGameCameraProvider>();

            builder.Register<CameraRayProvider>(Lifetime.Singleton)
                .As<ICameraRayProvider>();

            builder.RegisterBuildCallback(container =>
            {
                container.Resolve<IGameCameraProvider>();
            });
        }
    }
}
