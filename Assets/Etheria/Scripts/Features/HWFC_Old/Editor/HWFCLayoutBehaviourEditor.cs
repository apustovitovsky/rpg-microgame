#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;


namespace Etheria.Features.HWFC_Old
{
    [CustomEditor(typeof(HWFCLayoutBehaviour))]
    public sealed class HWFCLayoutBehaviourEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var behaviour = (HWFCLayoutBehaviour)target;

            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(behaviour.ModuleSetData == null))
            {
                if (GUILayout.Button("Generate"))
                {
                    Undo.RegisterFullObjectHierarchyUndo(behaviour.gameObject, "Generate HWFC Layout");
                    var result = behaviour.Generate();
                    var debugAsset = SaveDebugResultAsset(behaviour, result);
                    behaviour.SetDebugResultAsset(debugAsset);
                    EditorUtility.SetDirty(behaviour);
                    if (debugAsset != null)
                    {
                        EditorUtility.SetDirty(debugAsset);
                    }
                    EditorSceneManager.MarkSceneDirty(behaviour.gameObject.scene);
                    Debug.LogFormat(
                        behaviour,
                        "HWFC generate finished. Status: {0}, Size: {1}x{2}x{3}, Seed: {4}, ContradictionCellIndex: {5}",
                        result.Status,
                        result.Width,
                        result.Height,
                        result.Depth,
                        result.Seed,
                        result.ContradictionCellIndex);
                    GUIUtility.ExitGUI();
                }
            }

            if (GUILayout.Button("Clear"))
            {
                Undo.RegisterFullObjectHierarchyUndo(behaviour.gameObject, "Clear HWFC Layout");
                behaviour.Clear();
                EditorUtility.SetDirty(behaviour);
                EditorSceneManager.MarkSceneDirty(behaviour.gameObject.scene);
                GUIUtility.ExitGUI();
            }
        }

        private static LayoutGenerationDebugResultSO SaveDebugResultAsset(
            HWFCLayoutBehaviour behaviour,
            LayoutGenerationResult result)
        {
            if (behaviour.ModuleSetData == null)
            {
                return null;
            }

            var asset = behaviour.DebugResultAsset;
            if (asset == null)
            {
                var moduleSetPath = AssetDatabase.GetAssetPath(behaviour.ModuleSetData);
                var directory = Path.GetDirectoryName(moduleSetPath) ?? "Assets";
                var assetFileName = $"{SanitizeFileName(behaviour.gameObject.name)}.LayoutDebugResult.asset";
                var assetPath = AssetDatabase.GenerateUniqueAssetPath(
                    Path.Combine(directory, assetFileName).Replace("\\", "/"));

                asset = ScriptableObject.CreateInstance<LayoutGenerationDebugResultSO>();
                AssetDatabase.CreateAsset(asset, assetPath);
            }

            asset.Initialize(behaviour.ModuleSetData, result);
            AssetDatabase.SaveAssets();
            return asset;
        }

        private static string SanitizeFileName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "HWFCLayout";
            }

            foreach (var invalidChar in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(invalidChar, '_');
            }

            return value;
        }
    }
}
#endif
