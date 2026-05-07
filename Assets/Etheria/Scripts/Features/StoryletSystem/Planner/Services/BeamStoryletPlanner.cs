using System;
using System.Collections.Generic;
using System.Linq;

namespace Etheria.Features.StoryletSystem
{
    public sealed class BeamStoryletPlanner : IStoryletPlanner
    {
        private readonly IStoryletInstantiationService _instantiationService;
        private readonly IStoryletEffectApplier _effectApplier;
        private readonly IStoryletRepeatabilityService _repeatabilityService;
        private readonly IStoryletSalienceEvaluator _salienceEvaluator;
        private readonly IStoryletScoringService _scoringService;
        private readonly int _beamWidth;
        private readonly int _maxDepth;

        public BeamStoryletPlanner(
            IStoryletInstantiationService instantiationService,
            IStoryletEffectApplier effectApplier,
            IStoryletRepeatabilityService repeatabilityService,
            IStoryletSalienceEvaluator salienceEvaluator,
            IStoryletScoringService scoringService,
            int beamWidth = 3,
            int maxDepth = 6)
        {
            _instantiationService = instantiationService;
            _effectApplier = effectApplier;
            _repeatabilityService = repeatabilityService;
            _salienceEvaluator = salienceEvaluator;
            _scoringService = scoringService;
            _beamWidth = beamWidth;
            _maxDepth = maxDepth;
        }

        public StoryletPlannerResult Plan(
            StoryletWorldState initialWorldState,
            IReadOnlyList<StoryletDefinition> storylets,
            StoryletPlannerMemory initialMemory = null)
        {
            var traceExpansions = new List<StoryletBeamExpansionTrace>();
            var activeBranches = new List<PlannerBranch>
            {
                new("root", initialWorldState, initialMemory ?? StoryletPlannerMemory.Empty, new List<StoryletPlannedStep>(), 0f)
            };

            for (var depth = 0; depth < _maxDepth; depth++)
            {
                var expandedBranches = new List<PlannerBranch>();
                var seen = new HashSet<string>(StringComparer.Ordinal);

                foreach (var branch in activeBranches)
                {
                    var candidates = CollectCandidates(branch.WorldState, branch.Memory, storylets);

                    foreach (var candidate in candidates)
                    {
                        var nextWorldState = _effectApplier.Apply(candidate, branch.WorldState);
                        var nextMemory = _repeatabilityService.Advance(branch.Memory, candidate, nextWorldState);
                        var futureCandidates = CollectCandidates(nextWorldState, nextMemory, storylets);
                        candidate.ScoreBreakdown = _scoringService.Evaluate(
                            candidate,
                            branch.WorldState,
                            nextWorldState,
                            branch.Memory,
                            futureCandidates.Count);

                        var nextSteps = new List<StoryletPlannedStep>(branch.Steps)
                        {
                            new StoryletPlannedStep(depth + 1, branch.WorldState, candidate, nextWorldState)
                        };
                        var nextBranch = new PlannerBranch(
                            $"{branch.BranchId}.{candidate.Definition.Key}",
                            nextWorldState,
                            nextMemory,
                            nextSteps,
                            branch.TotalScore + candidate.ScoreBreakdown.Total);
                        var dedupKey = nextWorldState.GetFingerprint() + "|" + nextMemory.GetFingerprint();

                        if (seen.Add(dedupKey))
                        {
                            expandedBranches.Add(nextBranch);
                        }
                    }
                }

                if (expandedBranches.Count == 0)
                {
                    break;
                }

                var ordered = expandedBranches
                    .OrderByDescending(branch => branch.TotalScore)
                    .ThenBy(branch => branch.WorldState.SnapshotId)
                    .ToList();
                var kept = ordered.Take(_beamWidth).ToList();
                var pruned = ordered.Skip(_beamWidth).ToList();

                traceExpansions.Add(new StoryletBeamExpansionTrace(
                    depth + 1,
                    _beamWidth,
                    ordered.Select(branch => ToBeamTrace(branch)).ToList(),
                    kept.Select(branch => ToBeamTrace(branch, "kept")).ToList(),
                    pruned.Select(branch => ToBeamTrace(branch, "pruned_by_beam")).ToList()));

                activeBranches = kept;
            }

            var winningBranch = activeBranches
                .OrderByDescending(branch => branch.TotalScore)
                .FirstOrDefault()
                ?? new PlannerBranch("root", initialWorldState, initialMemory ?? StoryletPlannerMemory.Empty, new List<StoryletPlannedStep>(), 0f);

            var stepTraces = BuildWinningStepTraces(winningBranch, traceExpansions, storylets);
            return new StoryletPlannerResult(
                winningBranch.Steps,
                new StoryletPlannerTrace(stepTraces, traceExpansions));
        }

        private List<StoryletInstantiationCandidate> CollectCandidates(
            StoryletWorldState worldState,
            StoryletPlannerMemory memory,
            IReadOnlyList<StoryletDefinition> storylets)
        {
            var candidates = new List<StoryletInstantiationCandidate>();

            foreach (var definition in storylets)
            {
                if (_repeatabilityService.IsBlocked(definition, worldState, memory, out _))
                {
                    continue;
                }

                var result = _instantiationService.TryInstantiate(definition, worldState, memory);

                if (!result.IsValid)
                {
                    continue;
                }

                var candidate = result.Candidate;
                candidate.Salience = _salienceEvaluator.Evaluate(definition, candidate.Assignment, memory);
                candidates.Add(candidate);
            }

            return candidates;
        }

        private List<StoryletPlannerStepTrace> BuildWinningStepTraces(
            PlannerBranch winningBranch,
            IReadOnlyList<StoryletBeamExpansionTrace> beamExpansions,
            IReadOnlyList<StoryletDefinition> storylets)
        {
            var traces = new List<StoryletPlannerStepTrace>(winningBranch.Steps.Count);
            var memory = StoryletPlannerMemory.Empty;

            foreach (var step in winningBranch.Steps)
            {
                var candidates = new List<StoryletCandidateTrace>();

                foreach (var definition in storylets)
                {
                    var status = "invalid";
                    StoryletInstantiationCandidate candidate = null;
                    StoryletRejectionReason rejectionReason = null;

                    if (_repeatabilityService.IsBlocked(definition, step.BeforeWorldState, memory, out rejectionReason))
                    {
                        candidates.Add(new StoryletCandidateTrace(
                            definition.Key,
                            false,
                            status,
                            string.Empty,
                            null,
                            rejectionReason));
                        continue;
                    }

                    var result = _instantiationService.TryInstantiate(
                        definition,
                        step.BeforeWorldState,
                        memory);

                    if (result.IsValid)
                    {
                        candidate = result.Candidate;
                        candidate.Salience = _salienceEvaluator.Evaluate(definition, candidate.Assignment, memory);
                        candidate.ScoreBreakdown = candidate.Definition.Id == step.Candidate.Definition.Id
                            ? step.Candidate.ScoreBreakdown
                            : new StoryletScoreBreakdown(candidate.InstantiationQuality, definition.Priority * 10f, 0f, candidate.Salience.AntiRepetitionPenalty);
                        status = candidate.Definition.Id == step.Candidate.Definition.Id
                            ? "winning_branch_step"
                            : "valid_not_selected";
                    }
                    else
                    {
                        rejectionReason = result.RejectionReason;
                    }

                    candidates.Add(new StoryletCandidateTrace(
                        definition.Key,
                        candidate != null,
                        status,
                        candidate == null ? string.Empty : FormatAssignment(candidate.Assignment),
                        candidate?.ScoreBreakdown,
                        rejectionReason));
                }

                var survivingBranches = beamExpansions.Count >= step.StepNumber
                    ? beamExpansions[step.StepNumber - 1].KeptBranches
                    : Array.Empty<StoryletBeamBranchTrace>();

                traces.Add(new StoryletPlannerStepTrace(
                    step.StepNumber,
                    step.BeforeWorldState.SnapshotId,
                    candidates,
                    survivingBranches,
                    step));

                memory = _repeatabilityService.Advance(memory, step.Candidate, step.AfterWorldState);
            }

            return traces;
        }

        private static StoryletBeamBranchTrace ToBeamTrace(PlannerBranch branch, string reason = "")
        {
            var storyletKey = branch.Steps.Count == 0
                ? string.Empty
                : branch.Steps[branch.Steps.Count - 1].Candidate.Definition.Key;

            return new StoryletBeamBranchTrace(
                branch.BranchId,
                branch.TotalScore,
                branch.WorldState.SnapshotId,
                reason,
                storyletKey);
        }

        private static string FormatAssignment(IReadOnlyList<RoleAssignment> assignment)
        {
            return string.Join(
                ", ",
                assignment.Select(roleAssignment => $"{roleAssignment.Role.Key}->{roleAssignment.Entity.Key}"));
        }

        private sealed class PlannerBranch
        {
            public PlannerBranch(
                string branchId,
                StoryletWorldState worldState,
                StoryletPlannerMemory memory,
                List<StoryletPlannedStep> steps,
                float totalScore)
            {
                BranchId = branchId;
                WorldState = worldState;
                Memory = memory;
                Steps = steps;
                TotalScore = totalScore;
            }

            public string BranchId { get; }
            public StoryletWorldState WorldState { get; }
            public StoryletPlannerMemory Memory { get; }
            public List<StoryletPlannedStep> Steps { get; }
            public float TotalScore { get; }
        }
    }
}
