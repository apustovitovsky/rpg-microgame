using UnityEngine;

namespace Etheria.Core.DI
{
    public sealed class ScopeRoot
    {
        public ScopeRoot(Transform transform)
        {
            Transform = transform;
        }

        public Transform Transform { get; }
    }
}
