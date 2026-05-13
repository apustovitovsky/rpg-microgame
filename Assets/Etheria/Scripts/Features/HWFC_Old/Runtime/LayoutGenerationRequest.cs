using System;


namespace Etheria.Features.HWFC_Old
{
    [Serializable]
    public struct LayoutGenerationRequest
    {
        public ModuleSetData ModuleSetData;
        public int Width;
        public int Height;
        public int Depth;
        public int Seed;
    }

    public enum LayoutGenerationStatus
    {
        Success = 0,
        Contradiction = 1,
        InvalidInput = 2
    }

    [Serializable]
    public sealed class LayoutGenerationResult
    {
        public LayoutGenerationStatus Status;
        public int Width;
        public int Height;
        public int Depth;
        public int Seed;
        public int ContradictionCellIndex;
        public int[] CollapsedVariantIndices = Array.Empty<int>();

        public bool IsSuccess
        {
            get { return Status == LayoutGenerationStatus.Success; }
        }
    }
}
