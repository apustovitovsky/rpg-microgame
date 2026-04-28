using Etheria.Game.Camera;
using UnityEngine;

namespace Etheria.Features.Targeting
{
    public sealed class CameraRayProvider : ICameraRayProvider
    {
        private readonly ICameraTransformProvider _cameraProvider;

        public CameraRayProvider(ICameraTransformProvider cameraProvider)
        {
            _cameraProvider = cameraProvider;
        }

        public Ray GetForwardRay()
        {
            return new Ray(_cameraProvider.Position, _cameraProvider.Forward);
        }
    }
}