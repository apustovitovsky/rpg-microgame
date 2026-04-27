using UnityEngine;
using VContainer.Unity;

namespace Etheria.Core.DI
{
    public sealed class SceneScopeContext
    {
        public SceneDefinitionSO Definition { get; }
        public GameObject Root { get; }
        public LifetimeScope Scope { get; private set; }

        public Transform RootTransform => Root.transform;

        public SceneScopeContext(
            SceneDefinitionSO definition,
            GameObject root,
            LifetimeScope scope = null)
        {
            Definition = definition;
            Root = root;
            Scope = scope;
        }

        public void AttachScope(LifetimeScope scope)
        {
            Scope = scope;
        }
    }
}
