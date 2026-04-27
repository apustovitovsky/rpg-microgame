using System;
using UnityEngine;

namespace Etheria.Core.DI
{
    [Serializable]
    public sealed class SceneCatalogEntry
    {
        [field: SerializeField] public string DisplayName { get; private set; }

        [field: SerializeField] public SceneDefinitionSO[] SceneStack { get; private set; }

        [field: SerializeField] public SceneDefinitionSO ActiveScene { get; private set; }

        public int ActiveSceneIndex
        {
            get
            {
                if (SceneStack == null || SceneStack.Length == 0)
                    return -1;

                if (ActiveScene == null)
                    return 0;

                for (var i = 0; i < SceneStack.Length; i++)
                {
                    var definition = SceneStack[i];

                    if (definition == null || definition.ScenePath == null)
                        continue;

                    if (definition.ScenePath == ActiveScene.ScenePath)
                        return i;
                }

                return -1;
            }
        }
    }
}
