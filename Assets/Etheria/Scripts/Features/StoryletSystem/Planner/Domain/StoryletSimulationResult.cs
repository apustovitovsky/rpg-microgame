using System;
using System.Collections.Generic;

namespace Etheria.Features.StoryletSystem
{
    public sealed class StoryletSimulationResult
    {
        public StoryletSimulationResult(
            IReadOnlyList<StoryletPlannedStep> steps,
            StoryletWorldState initialWorldState,
            StoryletWorldState finalWorldState,
            float totalScore,
            StoryletDiagnosticsResult diagnostics = null)
        {
            Steps = steps ?? throw new ArgumentNullException(nameof(steps));
            InitialWorldState = initialWorldState ?? throw new ArgumentNullException(nameof(initialWorldState));
            FinalWorldState = finalWorldState ?? throw new ArgumentNullException(nameof(finalWorldState));
            TotalScore = totalScore;
            Diagnostics = diagnostics;
        }

        public IReadOnlyList<StoryletPlannedStep> Steps { get; }
        public StoryletWorldState InitialWorldState { get; }
        public StoryletWorldState FinalWorldState { get; }
        public float TotalScore { get; }
        public StoryletDiagnosticsResult Diagnostics { get; }
    }
}
