using UnityEngine;

namespace Etheria.Game.Camera
{
    public interface ICameraRayProvider
    {
        Ray GetForwardRay();
    }
}