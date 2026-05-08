using System;
using System.Collections.Generic;
using System.Linq;

namespace Etheria.Features.StoryletSystem
{
    public sealed class StoryletTelemetryFormatter : IStoryletTelemetryFormatter
    {
        public string Format(StoryletPlannerResult result)
        {
            var lines = new List<string>
            {
                "=== Storylet Planner Smoke ===",
                $"Winning steps: {result.WinningSteps.Count}",
                string.Empty
            };

            foreach (var stepTrace in result.Trace.StepTraces)
            {
                lines.Add($"Step {stepTrace.StepNumber}");
                lines.Add($"Current world snapshot id: {stepTrace.CurrentSnapshotId}");
                lines.Add("Candidates:");

                foreach (var candidate in stepTrace.Candidates)
                {
                    if (!candidate.IsValid)
                    {
                        lines.Add($"  - {candidate.StoryletKey}: invalid [{candidate.RejectionReason}]");
                        continue;
                    }

                    lines.Add(
                        $"  - {candidate.StoryletKey}: {candidate.SelectionStatus}; assignment={candidate.AssignmentSummary}; score={candidate.ScoreBreakdown.Total:0.##}");
                }

                lines.Add("Beam survivors:");

                foreach (var branch in stepTrace.SurvivingBranches)
                {
                    lines.Add(
                        $"  - branch={branch.BranchId}; storylet={branch.StoryletKey}; snapshot={branch.SnapshotId}; score={branch.TotalScore:0.##}; reason={branch.Reason}");
                }

                if (stepTrace.SelectedStep != null)
                {
                    lines.Add($"Winning branch step: {stepTrace.SelectedStep.Candidate.Definition.Key}");
                    lines.Add($"Winning branch assignment: {FormatAssignment(stepTrace.SelectedStep.Candidate.Assignment)}");
                    lines.Add(
                        $"Score breakdown: instantiation={stepTrace.SelectedStep.Candidate.ScoreBreakdown.InstantiationQuality:0.##}, transition={stepTrace.SelectedStep.Candidate.ScoreBreakdown.TransitionValue:0.##}, future={stepTrace.SelectedStep.Candidate.ScoreBreakdown.FuturePotential:0.##}, anti-repetition={stepTrace.SelectedStep.Candidate.ScoreBreakdown.AntiRepetition:0.##}, total={stepTrace.SelectedStep.Candidate.ScoreBreakdown.Total:0.##}");
                    lines.Add($"Applied effects: {FormatEffects(stepTrace.SelectedStep.Candidate.EffectPreview.Effects)}");
                    lines.Add($"Next world snapshot id: {stepTrace.SelectedStep.AfterWorldState.SnapshotId}");
                }

                lines.Add(string.Empty);
            }

            lines.Add("Beam expansions:");

            foreach (var expansion in result.Trace.BeamExpansions)
            {
                lines.Add($"Depth {expansion.Depth} / beam width {expansion.BeamWidth}");

                foreach (var branch in expansion.PrunedBranches)
                {
                    lines.Add(
                        $"  - pruned branch={branch.BranchId}; storylet={branch.StoryletKey}; snapshot={branch.SnapshotId}; score={branch.TotalScore:0.##}; reason={branch.Reason}");
                }
            }

            return string.Join(Environment.NewLine, lines);
        }

        private static string FormatAssignment(IReadOnlyList<RoleAssignment> assignment)
        {
            return string.Join(
                ", ",
                assignment.Select(roleAssignment => $"{roleAssignment.Role.Key}->{roleAssignment.Entity.Key}"));
        }

        private static string FormatEffects(IReadOnlyList<StoryletEffect> effects)
        {
            return string.Join(", ", effects.Select(effect => effect.GetType().Name));
        }
    }
}
