using Etheria.Game.Camera;
using UnityEngine;

namespace Etheria.Features.Targeting
{
    public sealed class ViewRayProvider : IViewRayProvider
    {
        private readonly ICameraTransformProvider _cameraProvider;

        public ViewRayProvider(ICameraTransformProvider cameraProvider)
        {
            _cameraProvider = cameraProvider;
        }

        public Ray GetRay()
        {
            return new Ray(_cameraProvider.Position, _cameraProvider.Forward);
        }
    }
}