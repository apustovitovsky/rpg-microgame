#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace RPG.Core.VContainer
{
    [CreateAssetMenu(
        menuName = "RPG/Core/Scene Loading/Scene Definition",
        fileName = "SceneDefinition")]
    public sealed class SceneDefinitionSO : ScriptableObject
    {
#if UNITY_EDITOR
        [SerializeField] private SceneAsset _scene;
#endif

        [SerializeField, ReadOnly] private string _scenePath;
        [field: SerializeField] public InstallerSO[] ExtraInstallers { get; private set; }

        public string ScenePath => _scenePath;

        public bool IsValid => !string.IsNullOrWhiteSpace(_scenePath);

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_scene == null)
            {
                _scenePath = string.Empty;
                return;
            }

            _scenePath = AssetDatabase.GetAssetPath(_scene);
        }
#endif
    }
}