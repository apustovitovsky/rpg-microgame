#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


namespace Etheria.Features.HWFC_Old
{
    public static class ModuleSetDataBaker
    {
        private const string BakeMenuPath = "Etheria/HWFC/Bake Selected Module Catalog";

        [MenuItem(BakeMenuPath)]
        public static void BakeSelected()
        {
            var catalog = Selection.activeObject as ModuleCatalogSO;

            if (catalog == null)
            {
                EditorUtility.DisplayDialog("HWFC Bake", "Select a ModuleCatalogSO asset before baking.", "OK");
                return;
            }

            Bake(catalog);
        }

        [MenuItem(BakeMenuPath, true)]
        public static bool CanBakeSelected()
        {
            return Selection.activeObject is ModuleCatalogSO;
        }

        [MenuItem("CONTEXT/ModuleCatalogSO/Bake Module Set Data")]
        public static void BakeFromContext(MenuCommand command)
        {
            var catalog = command.context as ModuleCatalogSO;
            if (catalog != null)
            {
                Bake(catalog);
            }
        }

        private static void Bake(ModuleCatalogSO catalog)
        {
            var modules = catalog.Items;
            var variants = BuildVariants(modules);
            var moduleCount = variants.Count;
            var directionCount = 6;

            if (moduleCount <= 0)
            {
                EditorUtility.DisplayDialog("HWFC Bake", "Module catalog produced no variants.", "OK");
                return;
            }

            if (moduleCount > ModuleSet.Capacity)
            {
                EditorUtility.DisplayDialog("HWFC Bake", "Module count exceeds ModuleSet capacity.", "OK");
                return;
            }

            var constraints = new ModuleSet[moduleCount * directionCount];
            var probabilities = new float[moduleCount];

            for (var moduleIndex = 0; moduleIndex < moduleCount; moduleIndex++)
            {
                var module = variants[moduleIndex];
                probabilities[moduleIndex] = module.Probability;

                for (var direction = 0; direction < directionCount; direction++)
                {
                    var allowed = ModuleSet.Empty(moduleCount);

                    for (var neighborIndex = 0; neighborIndex < moduleCount; neighborIndex++)
                    {
                        if (Fits(module, direction, variants[neighborIndex]))
                        {
                            allowed.Add(neighborIndex);
                        }
                    }

                    constraints[moduleIndex * directionCount + direction] = allowed;
                }
            }

            var asset = CreateOrLoadAssetFor(catalog);
            asset.Initialize(moduleCount, directionCount, variants.ToArray(), constraints, probabilities);

            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = asset;
        }

        private static ModuleSetData CreateOrLoadAssetFor(ModuleCatalogSO catalog)
        {
            var catalogPath = AssetDatabase.GetAssetPath(catalog);
            var directory = Path.GetDirectoryName(catalogPath) ?? "Assets";
            var assetPath = Path.Combine(directory, catalog.name + ".ModuleSetData.asset").Replace("\\", "/");

            var existing = AssetDatabase.LoadAssetAtPath<ModuleSetData>(assetPath);
            if (existing != null)
            {
                return existing;
            }

            var asset = ScriptableObject.CreateInstance<ModuleSetData>();
            AssetDatabase.CreateAsset(asset, assetPath);
            return asset;
        }

        private static List<ModuleVariantData> BuildVariants(IReadOnlyList<ModuleDataSO> modules)
        {
            var variants = new List<ModuleVariantData>();

            for (var moduleIndex = 0; moduleIndex < modules.Count; moduleIndex++)
            {
                var module = modules[moduleIndex];
                if (module == null)
                {
                    throw new InvalidOperationException("Module catalog contains a null module.");
                }

                ValidateModule(module);

                for (var rotation = 0; rotation < 4; rotation++)
                {
                    if (rotation != 0 && (!module.AllowRotations || CompareRotatedVariants(module, 0, rotation)))
                    {
                        continue;
                    }

                    variants.Add(CreateVariant(module, moduleIndex, rotation));
                }
            }

            return variants;
        }

        private static void ValidateModule(ModuleDataSO module)
        {
            if (module == null)
            {
                throw new InvalidOperationException("Module catalog contains a null module.");
            }
        }

        private static ModuleVariantData CreateVariant(ModuleDataSO module, int moduleIndex, int rotation)
        {
            var variant = new ModuleVariantData();
            variant.Initialize(
                module.name,
                moduleIndex,
                module.LayoutFamily,
                rotation,
                module.Probability,
                LayoutSemanticRules.GetRotatedConnectors(module.LayoutFamily, rotation));
            return variant;
        }

        private static bool CompareRotatedVariants(ModuleDataSO module, int rotationA, int rotationB)
        {
            var connectorsA = LayoutSemanticRules.GetRotatedConnectors(module.LayoutFamily, rotationA);
            var connectorsB = LayoutSemanticRules.GetRotatedConnectors(module.LayoutFamily, rotationB);

            for (var direction = 0; direction < 6; direction++)
            {
                if (connectorsA[direction] != connectorsB[direction])
                {
                    return false;
                }
            }

            return true;
        }

        private static bool Fits(ModuleVariantData source, int direction, ModuleVariantData target)
        {
            var otherDirection = GetOppositeDirection(direction);
            return LayoutSemanticRules.CanConnect(source.GetConnector(direction), target.GetConnector(otherDirection));
        }

        private static int GetOppositeDirection(int direction)
        {
            switch (direction)
            {
                case Orientations.LEFT:
                    return Orientations.RIGHT;
                case Orientations.DOWN:
                    return Orientations.UP;
                case Orientations.BACK:
                    return Orientations.FORWARD;
                case Orientations.RIGHT:
                    return Orientations.LEFT;
                case Orientations.UP:
                    return Orientations.DOWN;
                case Orientations.FORWARD:
                    return Orientations.BACK;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction));
            }
        }
    }
}
#endif
