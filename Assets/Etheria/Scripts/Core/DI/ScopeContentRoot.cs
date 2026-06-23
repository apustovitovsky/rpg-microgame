using UnityEngine;

namespace Etheria.Core.DI
{
    public sealed class ScopeContentRoot
    {
        public ScopeContentRoot(Transform transform)
        {
            Transform = transform;
        }

        public Transform Transform { get; }
    }
}
