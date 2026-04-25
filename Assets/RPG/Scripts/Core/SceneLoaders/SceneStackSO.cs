using System;
using UnityEngine;

namespace RPG.Core
{
    [CreateAssetMenu(
        fileName = "SceneStack",
        menuName = "RPG/Core/Scene Loading/Scene Stack")]
    public sealed class SceneStackSO : ScriptableObject
    {
        [Serializable]
        public struct AdditiveSceneDefinition
        {
            [SerializeField] private string _scenePath;
            [SerializeField] private bool _setActive;

            public string ScenePath => _scenePath;
            public bool SetActive => _setActive;
        }

        [SerializeField] private string _rootScenePath;
        [SerializeField] private AdditiveSceneDefinition[] _additiveScenes;

        public string RootScenePath => _rootScenePath;
        public AdditiveSceneDefinition[] AdditiveScenes => _additiveScenes;
    }
}
