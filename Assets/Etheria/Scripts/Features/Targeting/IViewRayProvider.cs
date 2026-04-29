using UnityEngine;

namespace Etheria.Features.Targeting
{
    public interface IViewRayProvider
    {
        Ray GetRay();
    }
}