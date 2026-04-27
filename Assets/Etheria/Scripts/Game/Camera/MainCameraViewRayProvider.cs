using UnityEngine;

namespace Etheria.Game.Camera
{
    public interface IViewRayProvider
    {
        Ray GetViewRay();
    }

    public sealed class MainCameraViewRayProvider : IViewRayProvider
    {
        private readonly IMainCameraProvider _camera;

        public MainCameraViewRayProvider(IMainCameraProvider camera)
        {
            _camera = camera;
        }

        public Ray GetViewRay()
        {
            var transform = _camera.Transform;
            return new Ray(transform.position, transform.forward);
        }
    }
}