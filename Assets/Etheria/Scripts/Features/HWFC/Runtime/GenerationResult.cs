using System;

namespace Etheria.Features.HWFC
{
    public sealed class GenerationResult
    {
        public GenerationResult(int[] selectedModules, int contradictionNodeIndex)
        {
            SelectedModules = selectedModules ?? throw new ArgumentNullException(nameof(selectedModules));
            ContradictionNodeIndex = contradictionNodeIndex;
        }

        public int[] SelectedModules { get; }

        public int ContradictionNodeIndex { get; }

        public bool HasContradiction => ContradictionNodeIndex >= 0;
    }
}
