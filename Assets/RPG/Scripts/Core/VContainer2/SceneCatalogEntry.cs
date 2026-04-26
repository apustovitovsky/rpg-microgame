using System;
using UnityEngine;

namespace RPG.Core.VContainer
{
    [Serializable]
    public sealed class SceneCatalogEntry
    {
        [field: SerializeField] public string DisplayName { get; private set; }
        [field: SerializeField] public SceneDefinitionSO[] SceneDefinitions { get; private set; }
    }
}