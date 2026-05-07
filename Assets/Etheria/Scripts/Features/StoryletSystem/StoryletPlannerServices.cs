using System;
using System.Collections.Generic;
using System.Linq;

namespace Etheria.Features.StoryletSystem
{
    public sealed class StoryletInstantiationService : IStoryletInstantiationService
    {
        private readonly IStoryletAssignmentBuilder _assignmentBuilder;
        private readonly IEntityRoleFitEvaluator _entityRoleFitEvaluator;

        public StoryletInstantiationService(
            IStoryletAssignmentBuilder assignmentBuilder,
            IEntityRoleFitEvaluator entityRoleFitEvaluator)
        {
            _assignmentBuilder = assignmentBuilder;
            _entityRoleFitEvaluator = entityRoleFitEvaluator;
        }

        public StoryletInstantiationResult TryInstantiate(
            StoryletDefinition definition,
            StoryletWorldState worldState,
            StoryletPlannerMemory memory)
        {
            foreach (var precondition in definition.Preconditions)
            {
                if (!precondition.IsSatisfied(worldState, memory, out var rejectionReason))
                {
                    return new StoryletInstantiationResult(false, null, rejectionReason);
                }
            }

            var legacyStorylet = definition.ToStorylet();
            var context = new StoryletMatchingContext(
                new[] { legacyStorylet },
                worldState.Entities,
                worldState.CreateRelationIndex());
            var freeEntities = new HashSet<Entity>(worldState.Entities);

            if (!_assignmentBuilder.TryBuildAssignment(
                context,
                legacyStorylet,
                freeEntities,
                out var assignment))
            {
                return new StoryletInstantiationResult(
                    false,
                    null,
                    new StoryletRejectionReason(
                        "invalid_role_binding",
                        $"No feasible assignment remained for '{definition.Key}'."));
            }

            var instantiationQuality = EvaluateInstantiationQuality(context, definition, assignment);
            var candidate = new StoryletInstantiationCandidate(
                definition,
                legacyStorylet,
                assignment,
                definition.Effects,
                instantiationQuality,
                StoryletSalienceEvaluationPlaceholder.Value);

            return new StoryletInstantiationResult(true, candidate, null);
        }

        private float EvaluateInstantiationQuality(
            StoryletMatchingContext context,
            StoryletDefinition definition,
            IReadOnlyList<RoleAssignment> assignment)
        {
            float fit = 0f;

            foreach (var roleAssignment in assignment)
            {
                fit += _entityRoleFitEvaluator.Evaluate(roleAssignment.Entity, roleAssignment.Role);
            }

            var relationScore = context.EvaluateAssignmentRelationScore(assignment);
            return definition.Priority * 5f + fit + relationScore * 10f;
        }

        private static class StoryletSalienceEvaluationPlaceholder
        {
            public static readonly StoryletSalienceEvaluation Value = new(0f, 0f);
        }
    }

    public sealed class StoryletRepeatabilityService : IStoryletRepeatabilityService
    {
        public bool IsBlocked(
            StoryletDefinition definition,
            StoryletWorldState worldState,
            StoryletPlannerMemory memory,
            out StoryletRejectionReason rejectionReason)
        {
            rejectionReason = null;
            var policy = definition.RepeatabilityPolicy;

            switch (policy.Mode)
            {
                case StoryletRepeatabilityMode.OnceEver:
                case StoryletRepeatabilityMode.OncePerRun:
                    if (memory.ExecutionCountByStoryletId.ContainsKey(definition.Id))
                    {
                        rejectionReason = new StoryletRejectionReason(
                            "repeatability_lock",
                            $"'{definition.Key}' is locked by {policy.Mode} policy.");
                        return true;
                    }

                    return false;
                case StoryletRepeatabilityMode.RepeatableWithCooldown:
                    if (memory.ExecutionStepByStoryletId.TryGetValue(definition.Id, out var lastStep)
                        && memory.CurrentStep - lastStep <= policy.CooldownSteps)
                    {
                        rejectionReason = new StoryletRejectionReason(
                            "repeatability_lock",
                            $"'{definition.Key}' is cooling down for {policy.CooldownSteps} steps.");
                        return true;
                    }

                    return false;
                case StoryletRepeatabilityMode.RepeatableUntilStateChanges:
                    if (memory.LastWorldFingerprintByStoryletId.TryGetValue(definition.Id, out var fingerprint)
                        && string.Equals(fingerprint, worldState.GetFingerprint(), StringComparison.Ordinal))
                    {
                        rejectionReason = new StoryletRejectionReason(
                            "repeatability_lock",
                            $"'{definition.Key}' already fired on the same world state.");
                        return true;
                    }

                    return false;
                default:
                    return false;
            }
        }

        public StoryletPlannerMemory Advance(
            StoryletPlannerMemory memory,
            StoryletInstantiationCandidate candidate,
            StoryletWorldState nextWorldState)
        {
            var executionSteps = new Dictionary<StoryletId, int>(memory.ExecutionStepByStoryletId);
            var executionCounts = new Dictionary<StoryletId, int>(memory.ExecutionCountByStoryletId);
            var pairingHistory = new Dictionary<string, int>(memory.LastStepByRolePairingKey, StringComparer.Ordinal);
            var lastFingerprints = new Dictionary<StoryletId, string>(memory.LastWorldFingerprintByStoryletId);
            var nextStep = memory.CurrentStep + 1;

            executionSteps[candidate.Definition.Id] = nextStep;
            executionCounts[candidate.Definition.Id] = executionCounts.TryGetValue(candidate.Definition.Id, out var count)
                ? count + 1
                : 1;
            lastFingerprints[candidate.Definition.Id] = nextWorldState.GetFingerprint();

            foreach (var roleAssignment in candidate.Assignment)
            {
                pairingHistory[BuildRolePairingKey(candidate.Definition.Id, roleAssignment)] = nextStep;
            }

            return new StoryletPlannerMemory(
                nextStep,
                executionSteps,
                executionCounts,
                pairingHistory,
                lastFingerprints);
        }

        public static string BuildRolePairingKey(StoryletId storyletId, RoleAssignment assignment)
        {
            return $"{storyletId.Value}:{assignment.Role.Id.Value}:{assignment.Entity.Id.Value}";
        }
    }

    public sealed class StoryletSalienceEvaluator : IStoryletSalienceEvaluator
    {
        public StoryletSalienceEvaluation Evaluate(
            StoryletDefinition definition,
            IReadOnlyList<RoleAssignment> assignment,
            StoryletPlannerMemory memory)
        {
            var bonus = definition.SaliencePolicy.BaseWeight + definition.SaliencePolicy.UnlockBonus;
            var antiRepetitionPenalty = 0f;

            if (memory.ExecutionStepByStoryletId.TryGetValue(definition.Id, out var lastStoryletStep))
            {
                antiRepetitionPenalty -= Math.Max(
                    0f,
                    definition.SaliencePolicy.RecentRepeatPenalty - (memory.CurrentStep - lastStoryletStep));
            }

            foreach (var roleAssignment in assignment)
            {
                var key = StoryletRepeatabilityService.BuildRolePairingKey(definition.Id, roleAssignment);

                if (memory.LastStepByRolePairingKey.TryGetValue(key, out var lastPairingStep))
                {
                    antiRepetitionPenalty -= Math.Max(
                        0f,
                        definition.SaliencePolicy.RepeatedPairPenalty - (memory.CurrentStep - lastPairingStep) * 0.5f);
                }
            }

            return new StoryletSalienceEvaluation(bonus, antiRepetitionPenalty);
        }
    }

    public sealed class StoryletEffectApplier : IStoryletEffectApplier
    {
        public StoryletWorldState Apply(
            StoryletInstantiationCandidate candidate,
            StoryletWorldState worldState)
        {
            var entityMap = new Dictionary<EntityId, Entity>();

            foreach (var entity in worldState.Entities)
            {
                entityMap[entity.Id] = entity;
            }

            var relationMap = new Dictionary<(EntityId From, EntityId To), TagSet>();

            foreach (var relation in worldState.Relations)
            {
                relationMap[(relation.FromEntityId, relation.ToEntityId)] = relation.Tags;
            }

            var worldAttributes = EnsureWorldAttributeCapacity(
                worldState.WorldAttributes.ToArray(),
                candidate.EffectPreview.Effects);

            foreach (var effect in candidate.EffectPreview.Effects)
            {
                ApplyEffect(effect, candidate.Assignment, entityMap, relationMap, worldAttributes);
            }

            return new StoryletWorldState(
                worldState.SnapshotId + 1,
                entityMap.Values.OrderBy(entity => entity.Id.Value).ToList(),
                relationMap.Select(pair => new EntityRelation(pair.Key.From, pair.Key.To, pair.Value)).ToList(),
                new AttributeSet(worldAttributes),
                worldState.WorldTags);
        }

        private static void ApplyEffect(
            StoryletEffect effect,
            IReadOnlyList<RoleAssignment> assignment,
            Dictionary<EntityId, Entity> entityMap,
            Dictionary<(EntityId From, EntityId To), TagSet> relationMap,
            float[] worldAttributes)
        {
            switch (effect)
            {
                case AddEntityTagEffect addEntityTag:
                    UpdateEntity(assignment, entityMap, addEntityTag.RoleId, entity => new Entity(
                        entity.Id,
                        entity.Key,
                        entity.Tags | addEntityTag.Tag,
                        entity.Attributes));
                    break;
                case RemoveEntityTagEffect removeEntityTag:
                    UpdateEntity(assignment, entityMap, removeEntityTag.RoleId, entity => new Entity(
                        entity.Id,
                        entity.Key,
                        entity.Tags.Without(removeEntityTag.Tag),
                        entity.Attributes));
                    break;
                case SetEntityAttributeEffect setEntityAttribute:
                    UpdateEntityAttribute(assignment, entityMap, setEntityAttribute.RoleId, setEntityAttribute.AttributeId, setEntityAttribute.Value, replace: true);
                    break;
                case AddEntityAttributeEffect addEntityAttribute:
                    UpdateEntityAttributeDelta(assignment, entityMap, addEntityAttribute.RoleId, addEntityAttribute.AttributeId, addEntityAttribute.Delta);
                    break;
                case RemoveEntityAttributeEffect removeEntityAttribute:
                    RemoveEntityAttribute(assignment, entityMap, removeEntityAttribute.RoleId, removeEntityAttribute.AttributeId);
                    break;
                case SetWorldAttributeEffect setWorldAttribute:
                    SetAttributeValue(worldAttributes, setWorldAttribute.AttributeId, setWorldAttribute.Value, replace: true);
                    break;
                case AddWorldAttributeEffect addWorldAttribute:
                    SetAttributeValue(
                        worldAttributes,
                        addWorldAttribute.AttributeId,
                        GetAttributeValue(worldAttributes, addWorldAttribute.AttributeId) + addWorldAttribute.Delta,
                        replace: true);
                    break;
                case SpawnEntityEffect spawnEntity:
                    entityMap[spawnEntity.Entity.Id] = spawnEntity.Entity;
                    break;
                case DespawnEntityEffect despawnEntity:
                    if (TryGetEntityId(assignment, despawnEntity.RoleId, out var entityId))
                    {
                        entityMap.Remove(entityId);

                        foreach (var relationKey in relationMap.Keys.ToList())
                        {
                            if (relationKey.From == entityId || relationKey.To == entityId)
                            {
                                relationMap.Remove(relationKey);
                            }
                        }
                    }
                    break;
                case CreateRelationEffect createRelation:
                    if (TryResolveRelationEndpoints(assignment, createRelation.FromRoleId, createRelation.ToRoleId, out var createKey))
                    {
                        relationMap[createKey] = createRelation.Tags;
                    }
                    break;
                case RemoveRelationEffect removeRelation:
                    if (TryResolveRelationEndpoints(assignment, removeRelation.FromRoleId, removeRelation.ToRoleId, out var removeKey))
                    {
                        relationMap.Remove(removeKey);
                    }
                    break;
                case AddRelationTagEffect addRelationTag:
                    if (TryResolveRelationEndpoints(assignment, addRelationTag.FromRoleId, addRelationTag.ToRoleId, out var addKey))
                    {
                        relationMap[addKey] = relationMap.TryGetValue(addKey, out var currentTags)
                            ? currentTags | addRelationTag.Tag
                            : addRelationTag.Tag;
                    }
                    break;
                case RemoveRelationTagEffect removeRelationTag:
                    if (TryResolveRelationEndpoints(assignment, removeRelationTag.FromRoleId, removeRelationTag.ToRoleId, out var removeTagKey)
                        && relationMap.TryGetValue(removeTagKey, out var existingTags))
                    {
                        relationMap[removeTagKey] = existingTags.Without(removeRelationTag.Tag);
                    }
                    break;
            }
        }

        private static void UpdateEntity(
            IReadOnlyList<RoleAssignment> assignment,
            Dictionary<EntityId, Entity> entityMap,
            RoleId roleId,
            Func<Entity, Entity> updater)
        {
            if (TryGetEntityId(assignment, roleId, out var entityId) && entityMap.TryGetValue(entityId, out var entity))
            {
                entityMap[entityId] = updater(entity);
            }
        }

        private static void UpdateEntityAttribute(
            IReadOnlyList<RoleAssignment> assignment,
            Dictionary<EntityId, Entity> entityMap,
            RoleId roleId,
            AttributeId attributeId,
            float value,
            bool replace)
        {
            UpdateEntity(assignment, entityMap, roleId, entity =>
            {
                var updated = entity.Attributes.ToArray();
                updated = EnsureCapacity(updated, attributeId);
                updated[attributeId.Value] = replace ? value : updated[attributeId.Value] + value;
                return new Entity(entity.Id, entity.Key, entity.Tags, new AttributeSet(updated));
            });
        }

        private static void UpdateEntityAttributeDelta(
            IReadOnlyList<RoleAssignment> assignment,
            Dictionary<EntityId, Entity> entityMap,
            RoleId roleId,
            AttributeId attributeId,
            float delta)
        {
            UpdateEntity(assignment, entityMap, roleId, entity =>
            {
                var updated = entity.Attributes.ToArray();
                updated = EnsureCapacity(updated, attributeId);
                updated[attributeId.Value] += delta;
                return new Entity(entity.Id, entity.Key, entity.Tags, new AttributeSet(updated));
            });
        }

        private static void RemoveEntityAttribute(
            IReadOnlyList<RoleAssignment> assignment,
            Dictionary<EntityId, Entity> entityMap,
            RoleId roleId,
            AttributeId attributeId)
        {
            UpdateEntity(assignment, entityMap, roleId, entity =>
            {
                var updated = entity.Attributes.ToArray();

                if (attributeId.Value >= 0 && attributeId.Value < updated.Length)
                {
                    updated[attributeId.Value] = 0f;
                }

                return new Entity(entity.Id, entity.Key, entity.Tags, new AttributeSet(updated));
            });
        }

        private static bool TryResolveRelationEndpoints(
            IReadOnlyList<RoleAssignment> assignment,
            RoleId fromRoleId,
            RoleId toRoleId,
            out (EntityId From, EntityId To) key)
        {
            if (TryGetEntityId(assignment, fromRoleId, out var fromEntityId)
                && TryGetEntityId(assignment, toRoleId, out var toEntityId))
            {
                key = (fromEntityId, toEntityId);
                return true;
            }

            key = default;
            return false;
        }

        private static bool TryGetEntityId(
            IReadOnlyList<RoleAssignment> assignment,
            RoleId roleId,
            out EntityId entityId)
        {
            foreach (var roleAssignment in assignment)
            {
                if (roleAssignment.Role.Id == roleId)
                {
                    entityId = roleAssignment.Entity.Id;
                    return true;
                }
            }

            entityId = default;
            return false;
        }

        private static float[] EnsureCapacity(float[] values, AttributeId attributeId)
        {
            if (attributeId.Value < values.Length)
            {
                return values;
            }

            var resized = new float[attributeId.Value + 1];
            Array.Copy(values, resized, values.Length);
            return resized;
        }

        private static void SetAttributeValue(
            float[] values,
            AttributeId attributeId,
            float value,
            bool replace)
        {
            if (attributeId.Value >= values.Length)
            {
                throw new InvalidOperationException("World attribute array capacity is insufficient.");
            }

            values[attributeId.Value] = replace ? value : values[attributeId.Value] + value;
        }

        private static float GetAttributeValue(float[] values, AttributeId attributeId)
        {
            return attributeId.Value >= 0 && attributeId.Value < values.Length
                ? values[attributeId.Value]
                : 0f;
        }

        private static float[] EnsureWorldAttributeCapacity(
            float[] worldAttributes,
            IReadOnlyList<StoryletEffect> effects)
        {
            var maxIndex = worldAttributes.Length - 1;

            foreach (var effect in effects)
            {
                switch (effect)
                {
                    case SetWorldAttributeEffect setWorldAttribute:
                        maxIndex = Math.Max(maxIndex, setWorldAttribute.AttributeId.Value);
                        break;
                    case AddWorldAttributeEffect addWorldAttribute:
                        maxIndex = Math.Max(maxIndex, addWorldAttribute.AttributeId.Value);
                        break;
                }
            }

            if (maxIndex < worldAttributes.Length)
            {
                return worldAttributes;
            }

            var resized = new float[maxIndex + 1];
            Array.Copy(worldAttributes, resized, worldAttributes.Length);
            return resized;
        }
    }

    public sealed class StoryletScoringService : IStoryletScoringService
    {
        public StoryletScoreBreakdown Evaluate(
            StoryletInstantiationCandidate candidate,
            StoryletWorldState currentWorldState,
            StoryletWorldState nextWorldState,
            StoryletPlannerMemory memory,
            int futureCandidateCount)
        {
            var transitionValue = candidate.Definition.Priority * 6f
                + candidate.Salience.Bonus * 3f
                + candidate.EffectPreview.Effects.Count * 1f;
            var futurePotential = futureCandidateCount * 3f;
            var antiRepetition = candidate.Salience.AntiRepetitionPenalty;

            return new StoryletScoreBreakdown(
                candidate.InstantiationQuality,
                transitionValue,
                futurePotential,
                antiRepetition);
        }
    }

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
