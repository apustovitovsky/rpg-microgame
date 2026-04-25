using System;
using UnityEngine;

namespace RPG.Core
{
    [CreateAssetMenu(
        fileName = "ExperienceDefinition",
        menuName = "RPG/Core/Experience Definition")]
    public sealed class ExperienceDefinitionSO : ScriptableObject
    {
        [Serializable]
        public struct SceneDefinition
        {
            [SerializeField] private string _scenePath;
            [SerializeField] private bool _setActive;
            [SerializeField] private InstallerSO[] _installers;

            public readonly string ScenePath => _scenePath;
            public readonly bool SetActive => _setActive;
            public readonly InstallerSO[] Installers => _installers;
        }

        [SerializeField] private SceneDefinition _rootScene;
        [SerializeField] private SceneDefinition[] _additiveScenes;

        public SceneDefinition RootScene => _rootScene;
        public SceneDefinition[] AdditiveScenes => _additiveScenes;
    }
}
