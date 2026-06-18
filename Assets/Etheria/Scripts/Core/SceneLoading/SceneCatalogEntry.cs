using System;
using UnityEngine;

namespace Etheria.Core.DI
{
    [Serializable]
    public sealed class SceneCatalogEntry
    {
        [field: SerializeField] public string DisplayName { get; private set; }

        [field: SerializeField] public SceneDefinitionSO[] SceneStack { get; private set; }

        public int ActiveSceneIndex
        {
            get
            {
                if (SceneStack == null || SceneStack.Length == 0)
                    return -1;

                var activeSceneIndex = -1;

                for (var i = 0; i < SceneStack.Length; i++)
                {
                    var definition = SceneStack[i];

                    if (definition == null)
                        continue;

                    if (activeSceneIndex < 0)
                        activeSceneIndex = i;

                    if (definition.SetActiveOnLoad)
                        activeSceneIndex = i;
                }

                return activeSceneIndex;
            }
        }
    }
}
