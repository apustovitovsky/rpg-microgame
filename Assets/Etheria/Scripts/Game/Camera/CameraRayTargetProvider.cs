using UnityEngine;

namespace Etheria.Game.Camera
{
    public sealed class CameraRayProvider : ICameraRayProvider
    {
        private readonly IGameCameraProvider _cameraProvider;

        public CameraRayProvider(IGameCameraProvider cameraProvider)
        {
            _cameraProvider = cameraProvider;
        }

        public Ray GetForwardRay()
        {
            var transform = _cameraProvider.Transform;
            return new Ray(transform.position, transform.forward);
        }
    }
}