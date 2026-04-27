using UnityEngine;

namespace RPG.Core.VContainer
{
    public sealed class SceneScopeContext
    {
        public SceneDefinitionSO Definition { get; }
        public GameObject Root { get; }

        public Transform RootTransform => Root.transform;

        public SceneScopeContext(
            SceneDefinitionSO definition,
            GameObject root)
        {
            Definition = definition;
            Root = root;
        }
    }
}

