using UnityEngine;

namespace Game.Core
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