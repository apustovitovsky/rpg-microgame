using UnityEngine;

namespace Etheria.Game.Camera
{
    public sealed class CameraRayTargetProvider : ICameraRayTargetProvider
    {
        private readonly IGameCameraProvider _cameraProvider;

        public CameraRayTargetProvider(IGameCameraProvider cameraProvider)
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