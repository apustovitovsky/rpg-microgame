using System;
using UnityEngine;


namespace Etheria.Features.HWFC_Old
{
    public sealed class HWFCLayoutBehaviour : MonoBehaviour
    {
        private const float CellSpacing = 1f;
        private const float CubeScale = 0.9f;

        [SerializeField] private ModuleSetData _moduleSetData;
        [SerializeField, Min(1)] private int _width = 12;
        [SerializeField, Min(1)] private int _height = 6;
        [SerializeField, Min(1)] private int _depth = 12;
        [SerializeField] private int _seed = 12345;
        [SerializeField] private LayoutFamilyVisualSettings[] _visualSettings = new LayoutFamilyVisualSettings[0];
        [SerializeField, HideInInspector] private Transform _previewRoot;
        [SerializeField, HideInInspector] private LayoutGenerationDebugResultSO _debugResultAsset;
        [SerializeField, HideInInspector] private LayoutGenerationStatus _lastStatus;
        [SerializeField, HideInInspector] private int _lastContradictionCellIndex = -1;
        [SerializeField, HideInInspector] private int[] _collapsedVariantIndices = new int[0];

        public ModuleSetData ModuleSetData
        {
            get { return _moduleSetData; }
            set { _moduleSetData = value; }
        }

        public int Width
        {
            get { return _width; }
            set { _width = Mathf.Max(1, value); }
        }

        public int Height
        {
            get { return _height; }
            set { _height = Mathf.Max(1, value); }
        }

        public int Depth
        {
            get { return _depth; }
            set { _depth = Mathf.Max(1, value); }
        }

        public int Seed
        {
            get { return _seed; }
            set { _seed = value; }
        }

        public LayoutFamilyVisualSettings[] VisualSettings => _visualSettings;
        public LayoutGenerationDebugResultSO DebugResultAsset => _debugResultAsset;

        public LayoutGenerationStatus LastStatus => _lastStatus;
        public int LastContradictionCellIndex => _lastContradictionCellIndex;
        public int[] CollapsedVariantIndices => _collapsedVariantIndices;

        public LayoutGenerationResult Generate()
        {
            Clear();

            var solver = new LayoutSolver();
            var result = solver.Solve(new LayoutGenerationRequest
            {
                ModuleSetData = _moduleSetData,
                Width = _width,
                Height = _height,
                Depth = _depth,
                Seed = _seed
            });

            _lastStatus = result.Status;
            _lastContradictionCellIndex = result.ContradictionCellIndex;
            _collapsedVariantIndices = result.CollapsedVariantIndices ?? new int[0];

            if (result.IsSuccess)
            {
                BuildPreview(result);
            }

            return result;
        }

        public void Clear()
        {
            _lastStatus = LayoutGenerationStatus.InvalidInput;
            _lastContradictionCellIndex = -1;
            _collapsedVariantIndices = new int[0];

            if (_previewRoot == null)
            {
                return;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                while (_previewRoot.childCount > 0)
                {
                    var child = _previewRoot.GetChild(_previewRoot.childCount - 1);
                    DestroyImmediate(child.gameObject);
                }

                DestroyImmediate(_previewRoot.gameObject);
                _previewRoot = null;
                return;
            }
#endif

            while (_previewRoot.childCount > 0)
            {
                var child = _previewRoot.GetChild(_previewRoot.childCount - 1);
                Destroy(child.gameObject);
            }

            Destroy(_previewRoot.gameObject);
            _previewRoot = null;
        }

        private void OnValidate()
        {
            _width = Mathf.Max(1, _width);
            _height = Mathf.Max(1, _height);
            _depth = Mathf.Max(1, _depth);
            EnsureDefaultVisualSettings();
        }

        private void BuildPreview(LayoutGenerationResult result)
        {
            _previewRoot = CreatePreviewRoot();

            for (var y = 0; y < result.Height; y++)
            {
                for (var z = 0; z < result.Depth; z++)
                {
                    for (var x = 0; x < result.Width; x++)
                    {
                        var cellIndex = x + result.Width * (z + result.Depth * y);
                        var variantIndex = result.CollapsedVariantIndices[cellIndex];
                        if (variantIndex < 0 || variantIndex >= _moduleSetData.ModuleCount)
                        {
                            continue;
                        }

                        var variant = _moduleSetData.Variants[variantIndex];
                        CreatePreviewCell(x, y, z, variantIndex, variant.LayoutFamily);
                    }
                }
            }
        }

        private Transform CreatePreviewRoot()
        {
            var previewRootObject = new GameObject("HWFC Preview");
            previewRootObject.transform.SetParent(transform, false);
            previewRootObject.transform.localPosition = Vector3.zero;
            previewRootObject.transform.localRotation = Quaternion.identity;
            previewRootObject.transform.localScale = Vector3.one;
            return previewRootObject.transform;
        }

        private void CreatePreviewCell(int x, int y, int z, int variantIndex, LayoutSemanticFamily family)
        {
            var cellObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cellObject.name = $"Cell_{x}_{y}_{z}_{family}_V{variantIndex}";
            cellObject.transform.SetParent(_previewRoot, false);
            cellObject.transform.localPosition = new Vector3(x * CellSpacing, y * CellSpacing, z * CellSpacing);

            var visual = GetVisualSettings(family);
            cellObject.transform.localScale = Vector3.one * (CubeScale * visual.Size);

            var renderer = cellObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                var material = CreatePreviewMaterial(visual.Color, visual.Alpha);
                if (material != null)
                {
                    renderer.sharedMaterial = material;
                }
            }

            var collider = cellObject.GetComponent<Collider>();
            if (collider != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    DestroyImmediate(collider);
                }
                else
#endif
                {
                    Destroy(collider);
                }
            }
        }

        private LayoutFamilyVisualSettings GetVisualSettings(LayoutSemanticFamily family)
        {
            if (_visualSettings != null)
            {
                for (var i = 0; i < _visualSettings.Length; i++)
                {
                    var item = _visualSettings[i];
                    if (item != null && item.Family == family)
                    {
                        return item;
                    }
                }
            }

            var fallback = new LayoutFamilyVisualSettings();
            fallback.Initialize(
                family,
                GetDefaultFamilyColor(family),
                GetDefaultFamilyAlpha(family),
                GetDefaultFamilySize(family));
            return fallback;
        }

        private static Material CreatePreviewMaterial(Color color, float alpha)
        {
            var shader =
                Shader.Find("Universal Render Pipeline/Unlit") ??
                Shader.Find("Universal Render Pipeline/Lit") ??
                Shader.Find("Universal Render Pipeline/Simple Lit") ??
                Shader.Find("Unlit/Color") ??
                Shader.Find("Standard");

            if (shader == null)
            {
                return null;
            }

            color.a = Mathf.Clamp01(alpha);
            var material = new Material(shader);
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }

            if (color.a < 0.999f)
            {
                ConfigureTransparentMaterial(material);
            }

            return material;
        }

        private void EnsureDefaultVisualSettings()
        {
            var families = (LayoutSemanticFamily[])Enum.GetValues(typeof(LayoutSemanticFamily));
            if (_visualSettings == null || _visualSettings.Length != families.Length)
            {
                _visualSettings = BuildDefaultVisualSettings(families);
                return;
            }

            for (var i = 0; i < families.Length; i++)
            {
                if (_visualSettings[i] == null || _visualSettings[i].Family != families[i])
                {
                    _visualSettings = BuildDefaultVisualSettings(families);
                    return;
                }
            }
        }

        private static LayoutFamilyVisualSettings[] BuildDefaultVisualSettings(LayoutSemanticFamily[] families)
        {
            var result = new LayoutFamilyVisualSettings[families.Length];
            for (var i = 0; i < families.Length; i++)
            {
                var item = new LayoutFamilyVisualSettings();
                item.Initialize(
                    families[i],
                    GetDefaultFamilyColor(families[i]),
                    GetDefaultFamilyAlpha(families[i]),
                    GetDefaultFamilySize(families[i]));
                result[i] = item;
            }

            return result;
        }

        private static Color GetDefaultFamilyColor(LayoutSemanticFamily family)
        {
            switch (family)
            {
                case LayoutSemanticFamily.Corner:
                    return new Color(0.85f, 0.45f, 0.35f);
                case LayoutSemanticFamily.Side:
                    return new Color(0.95f, 0.7f, 0.3f);
                case LayoutSemanticFamily.Center:
                    return new Color(0.75f, 0.25f, 0.25f);
                case LayoutSemanticFamily.Space:
                    return new Color(0.45f, 0.75f, 0.95f);
                case LayoutSemanticFamily.Air:
                    return new Color(0.85f, 0.9f, 1f);
                default:
                    return Color.magenta;
            }
        }

        private static float GetDefaultFamilyAlpha(LayoutSemanticFamily family)
        {
            switch (family)
            {
                case LayoutSemanticFamily.Air:
                    return 0.18f;
                case LayoutSemanticFamily.Space:
                    return 0.3f;
                default:
                    return 1f;
            }
        }

        private static float GetDefaultFamilySize(LayoutSemanticFamily family)
        {
            switch (family)
            {
                case LayoutSemanticFamily.Air:
                    return 0.18f;
                case LayoutSemanticFamily.Space:
                    return 0.28f;
                default:
                    return 1f;
            }
        }

        private static void ConfigureTransparentMaterial(Material material)
        {
            if (material.HasProperty("_Surface"))
            {
                material.SetFloat("_Surface", 1f);
            }

            if (material.HasProperty("_Blend"))
            {
                material.SetFloat("_Blend", 0f);
            }

            if (material.HasProperty("_SrcBlend"))
            {
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            }

            if (material.HasProperty("_DstBlend"))
            {
                material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            }

            if (material.HasProperty("_ZWrite"))
            {
                material.SetFloat("_ZWrite", 0f);
            }

            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }

#if UNITY_EDITOR
        internal void SetDebugResultAsset(LayoutGenerationDebugResultSO asset)
        {
            _debugResultAsset = asset;
        }
#endif
    }
}
