using UnityEngine;

namespace Etheria.Game.Camera
{
    public interface ICameraRayTargetProvider
    {
        Ray GetRay();
    }

    public sealed class CameraRayTargetProvider
    {
        private readonly IMainCameraProvider _cameraProvider;

        public CameraRayTargetProvider(IMainCameraProvider cameraProvider)
        {
            _cameraProvider = cameraProvider;
        }

        public Ray GetRay()
        {
            var transform = _cameraProvider.Transform;
            return new Ray(transform.position, transform.forward);
        }
    }
}