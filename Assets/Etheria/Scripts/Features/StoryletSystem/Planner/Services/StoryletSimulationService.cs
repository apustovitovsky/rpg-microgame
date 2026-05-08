using System;
using System.Linq;

namespace Etheria.Features.StoryletSystem
{
    public sealed class StoryletSimulationService : IStoryletSimulationService
    {
        private readonly IStoryletPlanner _planner;

        public StoryletSimulationService(IStoryletPlanner planner)
        {
            _planner = planner ?? throw new ArgumentNullException(nameof(planner));
        }

        public StoryletSimulationResult Simulate(StoryletSimulationRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var plannerResult = _planner.Plan(
                request.InitialWorldState,
                request.Storylets,
                request.InitialMemory,
                request.PlannerOptions);

            var finalWorldState = plannerResult.WinningSteps.Count == 0
                ? request.InitialWorldState
                : plannerResult.WinningSteps[plannerResult.WinningSteps.Count - 1].AfterWorldState;
            var totalScore = plannerResult.WinningSteps.Sum(step => step.Candidate.ScoreBreakdown?.Total ?? 0f);
            var diagnostics = request.IncludeDiagnostics
                ? new StoryletDiagnosticsResult(plannerResult.Trace)
                : null;

            return new StoryletSimulationResult(
                plannerResult.WinningSteps,
                request.InitialWorldState,
                finalWorldState,
                totalScore,
                diagnostics);
        }
    }
}
