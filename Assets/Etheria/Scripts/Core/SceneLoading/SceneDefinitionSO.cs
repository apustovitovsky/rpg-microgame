using UnityEngine;
using Etheria.Core.Helpers;



#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Etheria.Core.DI
{
    [CreateAssetMenu(
        menuName = "Etheria/Core/Scene Loading/Scene Definition",
        fileName = "SceneDefinition")]
    public sealed class SceneDefinitionSO : ScriptableObject
    {
#if UNITY_EDITOR
        [SerializeField] private SceneAsset _scene;
#endif

        [SerializeField, ReadOnlyField] private string _scenePath;
        [SerializeField] private bool _setActiveOnLoad;

        public bool IsValid => !string.IsNullOrWhiteSpace(_scenePath);
        public string ScenePath => _scenePath;
        public bool SetActiveOnLoad => _setActiveOnLoad;

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
