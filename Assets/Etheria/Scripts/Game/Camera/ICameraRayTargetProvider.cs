using UnityEngine;

namespace Etheria.Game.Camera
{
    public interface ICameraRayTargetProvider
    {
        Ray GetRay();
    }
}