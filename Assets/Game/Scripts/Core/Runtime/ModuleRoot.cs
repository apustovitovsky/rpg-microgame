using UnityEngine;

namespace Game.Core
{
    public sealed class ModuleRoot
    {
        public ModuleRoot(Transform transform)
        {
            Transform = transform;
        }

        public Transform Transform { get; }
    }
}