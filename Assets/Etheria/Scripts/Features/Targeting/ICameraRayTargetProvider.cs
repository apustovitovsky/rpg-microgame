using UnityEngine;

namespace Etheria.Features.Targeting
{
    public interface ICameraRayProvider
    {
        Ray GetForwardRay();
    }
}