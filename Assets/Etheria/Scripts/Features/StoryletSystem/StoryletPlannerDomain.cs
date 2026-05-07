using System;
using System.Collections.Generic;
using System.Linq;

namespace Etheria.Features.StoryletSystem
{
    public sealed class StoryletWorldState
    {
        private readonly Dictionary<EntityId, Entity> _entitiesById;

        public StoryletWorldState(
            int snapshotId,
            IReadOnlyList<Entity> entities,
            IReadOnlyList<EntityRelation> relations,
            AttributeSet worldAttributes,
            TagSet worldTags = default)
        {
            SnapshotId = snapshotId;
            Entities = entities ?? throw new ArgumentNullException(nameof(entities));
            Relations = relations ?? throw new ArgumentNullException(nameof(relations));
            WorldAttributes = worldAttributes;
            WorldTags = worldTags;
            _entitiesById = new Dictionary<EntityId, Entity>(entities.Count);

            foreach (var entity in entities)
            {
                if (entity != null)
                {
                    _entitiesById[entity.Id] = entity;
                }
            }
        }

        public int SnapshotId { get; }
        public IReadOnlyList<Entity> Entities { get; }
        public IReadOnlyList<EntityRelation> Relations { get; }
        public AttributeSet WorldAttributes { get; }
        public TagSet WorldTags { get; }

        public bool TryGetEntity(EntityId entityId, out Entity entity)
        {
            return _entitiesById.TryGetValue(entityId, out entity);
        }

        public bool ContainsEntity(EntityId entityId)
        {
            return _entitiesById.ContainsKey(entityId);
        }

        public bool HasAnyEntity(TagQuery query)
        {
            foreach (var entity in Entities)
            {
                if (entity != null && query.Matches(entity.Tags))
                {
                    return true;
                }
            }

            return false;
        }

        public int CountEntities(TagQuery query)
        {
            var count = 0;

            foreach (var entity in Entities)
            {
                if (entity != null && query.Matches(entity.Tags))
                {
                    count++;
                }
            }

            return count;
        }

        public RelationIndex CreateRelationIndex()
        {
            return new RelationIndex(Relations);
        }

        public string GetFingerprint()
        {
            var entityParts = new List<string>(Entities.Count);

            foreach (var entity in Entities.OrderBy(entity => entity.Id.Value))
            {
                entityParts.Add(
                    $"{entity.Id.Value}:{entity.Key}:{entity.Tags.GetHashCode()}:{GetAttributeFingerprint(entity.Attributes)}");
            }

            var relationParts = new List<string>(Relations.Count);

            foreach (var relation in Relations
                .OrderBy(relation => relation.FromEntityId.Value)
                .ThenBy(relation => relation.ToEntityId.Value))
            {
                relationParts.Add(
                    $"{relation.FromEntityId.Value}>{relation.ToEntityId.Value}:{relation.Tags.GetHashCode()}");
            }

            return string.Join("|", entityParts)
                + "#"
                + string.Join("|", relationParts)
                + "#"
                + WorldTags.GetHashCode()
                + "#"
                + GetAttributeFingerprint(WorldAttributes);
        }

        private static string GetAttributeFingerprint(AttributeSet attributes)
        {
            return string.Join(",", attributes.ToArray().Select(value => value.ToString("0.###")));
        }
    }

    public sealed class StoryletDefinition
    {
        public StoryletDefinition(
            StoryletId id,
            string key,
            float priority,
            IReadOnlyList<Role> roles,
            IReadOnlyList<IStoryletPrecondition> preconditions,
            StoryletEffectBatch effects,
            StoryletRepeatabilityPolicy repeatabilityPolicy,
            StoryletSaliencePolicy saliencePolicy)
        {
            Id = id;
            Key = key ?? throw new ArgumentNullException(nameof(key));
            Priority = priority;
            Roles = roles ?? throw new ArgumentNullException(nameof(roles));
            Preconditions = preconditions ?? Array.Empty<IStoryletPrecondition>();
            Effects = effects ?? throw new ArgumentNullException(nameof(effects));
            RepeatabilityPolicy = repeatabilityPolicy ?? StoryletRepeatabilityPolicy.OncePerRun();
            SaliencePolicy = saliencePolicy ?? StoryletSaliencePolicy.Default;
        }

        public StoryletId Id { get; }
        public string Key { get; }
        public float Priority { get; }
        public IReadOnlyList<Role> Roles { get; }
        public IReadOnlyList<IStoryletPrecondition> Preconditions { get; }
        public StoryletEffectBatch Effects { get; }
        public StoryletRepeatabilityPolicy RepeatabilityPolicy { get; }
        public StoryletSaliencePolicy SaliencePolicy { get; }

        public Storylet ToStorylet()
        {
            return new Storylet(Id, Key, Priority, Roles);
        }
    }

    public sealed class StoryletEffectBatch
    {
        public StoryletEffectBatch(IReadOnlyList<StoryletEffect> effects)
        {
            Effects = effects ?? throw new ArgumentNullException(nameof(effects));
        }

        public IReadOnlyList<StoryletEffect> Effects { get; }
    }

    public sealed class StoryletInstantiationResult
    {
        public StoryletInstantiationResult(
            bool isValid,
            StoryletInstantiationCandidate candidate,
            StoryletRejectionReason rejectionReason)
        {
            IsValid = isValid;
            Candidate = candidate;
            RejectionReason = rejectionReason;
        }

        public bool IsValid { get; }
        public StoryletInstantiationCandidate Candidate { get; }
        public StoryletRejectionReason RejectionReason { get; }
    }

    public sealed class StoryletInstantiationCandidate
    {
        public StoryletInstantiationCandidate(
            StoryletDefinition definition,
            Storylet legacyStorylet,
            IReadOnlyList<RoleAssignment> assignment,
            StoryletEffectBatch effectPreview,
            float instantiationQuality,
            StoryletSalienceEvaluation salience)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            LegacyStorylet = legacyStorylet ?? throw new ArgumentNullException(nameof(legacyStorylet));
            Assignment = assignment ?? throw new ArgumentNullException(nameof(assignment));
            EffectPreview = effectPreview ?? throw new ArgumentNullException(nameof(effectPreview));
            InstantiationQuality = instantiationQuality;
            Salience = salience ?? throw new ArgumentNullException(nameof(salience));
        }

        public StoryletDefinition Definition { get; }
        public Storylet LegacyStorylet { get; }
        public IReadOnlyList<RoleAssignment> Assignment { get; }
        public StoryletEffectBatch EffectPreview { get; }
        public float InstantiationQuality { get; }
        public StoryletSalienceEvaluation Salience { get; set; }
        public StoryletScoreBreakdown ScoreBreakdown { get; set; }
    }

    public enum StoryletRepeatabilityMode
    {
        OnceEver,
        OncePerRun,
        RepeatableWithCooldown,
        RepeatableUntilStateChanges
    }

    public sealed class StoryletRepeatabilityPolicy
    {
        public StoryletRepeatabilityPolicy(
            StoryletRepeatabilityMode mode,
            int cooldownSteps = 0)
        {
            Mode = mode;
            CooldownSteps = cooldownSteps;
        }

        public StoryletRepeatabilityMode Mode { get; }
        public int CooldownSteps { get; }

        public static StoryletRepeatabilityPolicy OnceEver()
        {
            return new StoryletRepeatabilityPolicy(StoryletRepeatabilityMode.OnceEver);
        }

        public static StoryletRepeatabilityPolicy OncePerRun()
        {
            return new StoryletRepeatabilityPolicy(StoryletRepeatabilityMode.OncePerRun);
        }

        public static StoryletRepeatabilityPolicy WithCooldown(int cooldownSteps)
        {
            return new StoryletRepeatabilityPolicy(
                StoryletRepeatabilityMode.RepeatableWithCooldown,
                cooldownSteps);
        }

        public static StoryletRepeatabilityPolicy UntilStateChanges()
        {
            return new StoryletRepeatabilityPolicy(StoryletRepeatabilityMode.RepeatableUntilStateChanges);
        }
    }

    public sealed class StoryletSaliencePolicy
    {
        public static readonly StoryletSaliencePolicy Default = new(0f, 0f, 3f, 0.5f);

        public StoryletSaliencePolicy(
            float baseWeight,
            float unlockBonus,
            float recentRepeatPenalty,
            float repeatedPairPenalty)
        {
            BaseWeight = baseWeight;
            UnlockBonus = unlockBonus;
            RecentRepeatPenalty = recentRepeatPenalty;
            RepeatedPairPenalty = repeatedPairPenalty;
        }

        public float BaseWeight { get; }
        public float UnlockBonus { get; }
        public float RecentRepeatPenalty { get; }
        public float RepeatedPairPenalty { get; }
    }

    public sealed class StoryletPlannerMemory
    {
        public StoryletPlannerMemory(
            int currentStep,
            IReadOnlyDictionary<StoryletId, int> executionStepByStoryletId,
            IReadOnlyDictionary<StoryletId, int> executionCountByStoryletId,
            IReadOnlyDictionary<string, int> lastStepByRolePairingKey,
            IReadOnlyDictionary<StoryletId, string> lastFingerprintByStoryletId)
        {
            CurrentStep = currentStep;
            ExecutionStepByStoryletId = executionStepByStoryletId
                ?? new Dictionary<StoryletId, int>();
            ExecutionCountByStoryletId = executionCountByStoryletId
                ?? new Dictionary<StoryletId, int>();
            LastStepByRolePairingKey = lastStepByRolePairingKey
                ?? new Dictionary<string, int>(StringComparer.Ordinal);
            LastWorldFingerprintByStoryletId = lastFingerprintByStoryletId
                ?? new Dictionary<StoryletId, string>();
        }

        public int CurrentStep { get; }
        public IReadOnlyDictionary<StoryletId, int> ExecutionStepByStoryletId { get; }
        public IReadOnlyDictionary<StoryletId, int> ExecutionCountByStoryletId { get; }
        public IReadOnlyDictionary<string, int> LastStepByRolePairingKey { get; }
        public IReadOnlyDictionary<StoryletId, string> LastWorldFingerprintByStoryletId { get; }

        public static StoryletPlannerMemory Empty { get; } = new(
            0,
            new Dictionary<StoryletId, int>(),
            new Dictionary<StoryletId, int>(),
            new Dictionary<string, int>(StringComparer.Ordinal),
            new Dictionary<StoryletId, string>());

        public string GetFingerprint()
        {
            var storyletHistory = string.Join(
                ",",
                ExecutionStepByStoryletId.OrderBy(pair => pair.Key.Value)
                    .Select(pair => $"{pair.Key.Value}:{pair.Value}"));
            var pairingHistory = string.Join(
                ",",
                LastStepByRolePairingKey.OrderBy(pair => pair.Key, StringComparer.Ordinal)
                    .Select(pair => $"{pair.Key}:{pair.Value}"));

            return $"{CurrentStep}#{storyletHistory}#{pairingHistory}";
        }
    }

    public sealed class StoryletScoreBreakdown
    {
        public StoryletScoreBreakdown(
            float instantiationQuality,
            float transitionValue,
            float futurePotential,
            float antiRepetition)
        {
            InstantiationQuality = instantiationQuality;
            TransitionValue = transitionValue;
            FuturePotential = futurePotential;
            AntiRepetition = antiRepetition;
        }

        public float InstantiationQuality { get; }
        public float TransitionValue { get; }
        public float FuturePotential { get; }
        public float AntiRepetition { get; }
        public float Total => InstantiationQuality + TransitionValue + FuturePotential + AntiRepetition;
    }

    public sealed class StoryletSalienceEvaluation
    {
        public StoryletSalienceEvaluation(float bonus, float antiRepetitionPenalty)
        {
            Bonus = bonus;
            AntiRepetitionPenalty = antiRepetitionPenalty;
        }

        public float Bonus { get; }
        public float AntiRepetitionPenalty { get; }
    }

    public sealed class StoryletRejectionReason
    {
        public StoryletRejectionReason(string category, string message)
        {
            Category = category ?? throw new ArgumentNullException(nameof(category));
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        public string Category { get; }
        public string Message { get; }

        public override string ToString()
        {
            return $"{Category}: {Message}";
        }
    }

    public sealed class StoryletPlannerResult
    {
        public StoryletPlannerResult(
            IReadOnlyList<StoryletPlannedStep> winningSteps,
            StoryletPlannerTrace trace)
        {
            WinningSteps = winningSteps ?? throw new ArgumentNullException(nameof(winningSteps));
            Trace = trace ?? throw new ArgumentNullException(nameof(trace));
        }

        public IReadOnlyList<StoryletPlannedStep> WinningSteps { get; }
        public StoryletPlannerTrace Trace { get; }
    }

    public sealed class StoryletPlannedStep
    {
        public StoryletPlannedStep(
            int stepNumber,
            StoryletWorldState beforeWorldState,
            StoryletInstantiationCandidate candidate,
            StoryletWorldState afterWorldState)
        {
            StepNumber = stepNumber;
            BeforeWorldState = beforeWorldState ?? throw new ArgumentNullException(nameof(beforeWorldState));
            Candidate = candidate ?? throw new ArgumentNullException(nameof(candidate));
            AfterWorldState = afterWorldState ?? throw new ArgumentNullException(nameof(afterWorldState));
        }

        public int StepNumber { get; }
        public StoryletWorldState BeforeWorldState { get; }
        public StoryletInstantiationCandidate Candidate { get; }
        public StoryletWorldState AfterWorldState { get; }
    }
}
