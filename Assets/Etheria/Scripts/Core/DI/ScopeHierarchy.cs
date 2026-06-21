using UnityEngine;

namespace Etheria.Core.DI
{
    public sealed class ScopeHierarchy
    {
        public ScopeHierarchy(Transform contentRoot)
        {
            ContentRoot = contentRoot;
        }

        public Transform ContentRoot { get; }
    }
}
