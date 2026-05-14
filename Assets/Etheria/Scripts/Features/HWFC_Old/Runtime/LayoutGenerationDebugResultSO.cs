using System;
using UnityEngine;


namespace Etheria.Features.HWFC_Old
{
    [CreateAssetMenu(
        menuName = "Etheria/HWFC_Old/Layout Generation Debug Result",
        fileName = "LayoutGenerationDebugResult")]
    public sealed class LayoutGenerationDebugResultSO : ScriptableObject
    {
        [SerializeField] private ModuleSetData _moduleSetData;
        [SerializeField] private LayoutGenerationStatus _status;
        [SerializeField] private int _width;
        [SerializeField] private int _height;
        [SerializeField] private int _depth;
        [SerializeField] private int _seed;
        [SerializeField] private int _contradictionCellIndex = -1;
        [SerializeField] private int[] _collapsedVariantIndices = Array.Empty<int>();

        public ModuleSetData ModuleSetData => _moduleSetData;
        public LayoutGenerationStatus Status => _status;
        public int Width => _width;
        public int Height => _height;
        public int Depth => _depth;
        public int Seed => _seed;
        public int ContradictionCellIndex => _contradictionCellIndex;
        public int[] CollapsedVariantIndices => _collapsedVariantIndices;

        public void Initialize(ModuleSetData moduleSetData, LayoutGenerationResult result)
        {
            _moduleSetData = moduleSetData;
            _status = result != null ? result.Status : LayoutGenerationStatus.InvalidInput;
            _width = result != null ? result.Width : 0;
            _height = result != null ? result.Height : 0;
            _depth = result != null ? result.Depth : 0;
            _seed = result != null ? result.Seed : 0;
            _contradictionCellIndex = result != null ? result.ContradictionCellIndex : -1;
            _collapsedVariantIndices = result?.CollapsedVariantIndices ?? Array.Empty<int>();
        }
    }
}
