using System.Collections.Generic;
using System.Text;
using UnityEngine;
using VContainer.Unity;

namespace Etheria.Features.StoryletSystem
{
    public sealed class StoryletMatcherSmokeTestService : IStartable
    {
        private static readonly AttributeId WealthAttributeId = new(0);

        private static readonly StoryletId SwornEscortStoryletId = new(0);
        private static readonly StoryletId SecretAudienceStoryletId = new(1);
        private static readonly StoryletId MutualRitualStoryletId = new(2);
        private static readonly StoryletId GuardedSermonStoryletId = new(3);
        private static readonly StoryletId CityPatrolStoryletId = new(4);
        private static readonly StoryletId ContrabandDropStoryletId = new(5);

        private static readonly RoleId NobleRoleId = new(0);
        private static readonly RoleId KnightRoleId = new(1);
        private static readonly RoleId PatronRoleId = new(2);
        private static readonly RoleId EnvoyRoleId = new(3);
        private static readonly RoleId RitualistRoleId = new(4);
        private static readonly RoleId WitnessRoleId = new(5);
        private static readonly RoleId PriestRoleId = new(6);
        private static readonly RoleId GuardRoleId = new(7);
        private static readonly RoleId PatrolGuardRoleId = new(8);
        private static readonly RoleId SmugglerRoleId = new(9);
        private static readonly RoleId LookoutRoleId = new(10);
        private static readonly RoleId RunnerRoleId = new(11);

        private static readonly EntityId NoblePatronEntityId = new(0);
        private static readonly EntityId SwornKnightEntityId = new(1);
        private static readonly EntityId RivalMercenaryEntityId = new(2);
        private static readonly EntityId CityGuardEntityId = new(3);
        private static readonly EntityId ScoutEnvoyEntityId = new(4);
        private static readonly EntityId SmugglerEntityId = new(5);
        private static readonly EntityId CourtPriestEntityId = new(6);
        private static readonly EntityId RitualAcolyteEntityId = new(7);
        private static readonly EntityId RitualMageEntityId = new(8);
        private static readonly EntityId OutlawScoutEntityId = new(9);

        private static readonly TagSet WarriorTag = TagSet.FromIndex(0);
        private static readonly TagSet BanditTag = TagSet.FromIndex(1);
        private static readonly TagSet PriestTag = TagSet.FromIndex(2);
        private static readonly TagSet ScoutTag = TagSet.FromIndex(3);
        private static readonly TagSet NobleTag = TagSet.FromIndex(4);
        private static readonly TagSet OutlawTag = TagSet.FromIndex(5);
        private static readonly TagSet RuralTag = TagSet.FromIndex(6);
        private static readonly TagSet UrbanTag = TagSet.FromIndex(7);
        private static readonly TagSet ArcaneTag = TagSet.FromIndex(8);
        private static readonly TagSet MercenaryTag = TagSet.FromIndex(9);

        private static readonly TagSet SwornAllyRelationTag = TagSet.FromIndex(100);
        private static readonly TagSet VassalRelationTag = TagSet.FromIndex(101);
        private static readonly TagSet TrustRelationTag = TagSet.FromIndex(102);
        private static readonly TagSet CovenantRelationTag = TagSet.FromIndex(103);
        private static readonly TagSet EnemyRelationTag = TagSet.FromIndex(104);

        private readonly GreedyStoryletMatcher _matcher;

        public StoryletMatcherSmokeTestService(GreedyStoryletMatcher matcher)
        {
            _matcher = matcher;
        }

        public StoryletMatchResult RunSample()
        {
            var entities = BuildSampleEntities();
            var storylets = BuildSampleStorylets();
            var relations = BuildSampleRelations();
            return _matcher.Match(entities, storylets, relations);
        }

        public void LogSample()
        {
            var entities = BuildSampleEntities();
            var storylets = BuildSampleStorylets();
            var relations = BuildSampleRelations();
            var result = _matcher.Match(entities, storylets, relations);
            Debug.Log(Describe(entities, relations, storylets, result));
        }

        public string Describe(
            IReadOnlyList<Entity> entities,
            IReadOnlyList<EntityRelation> relations,
            IReadOnlyList<Storylet> storylets,
            StoryletMatchResult result)
        {
            var builder = new StringBuilder();
            builder.AppendLine("=== Storylet Matcher Smoke Test ===");
            builder.AppendLine();
            builder.AppendLine($"Entities: {entities.Count}");

            foreach (var entity in entities)
            {
                builder.AppendLine(
                    $"  - {entity.Key,-24} [{DescribeTags(entity.Tags)}] [{DescribeAttributes(entity.Attributes)}]");
            }

            builder.AppendLine();
            builder.AppendLine($"Storylets: {storylets.Count}");

            foreach (var storylet in storylets)
            {
                builder.AppendLine();
                builder.AppendLine($"  {storylet.Key}  (priority {storylet.Priority:0.##})");

                foreach (var role in storylet.Roles)
                {
                    builder.Append($"    {role.Key,-20} <- ");

                    var foundAnyEntity = false;

                    foreach (var entity in entities)
                    {
                        if (!entity.CanFill(role))
                        {
                            continue;
                        }

                        if (foundAnyEntity)
                        {
                            builder.Append(", ");
                        }

                        builder.Append(entity.Key);
                        foundAnyEntity = true;
                    }

                    if (!foundAnyEntity)
                    {
                        builder.Append("no compatible entities");
                    }

                    builder.AppendLine();
                }
            }

            builder.AppendLine();
            builder.AppendLine($"Relations: {relations.Count}");

            foreach (var relation in relations)
            {
                builder.AppendLine(
                    $"  - {DescribeEntityId(relation.FromEntityId),-24} -> {DescribeEntityId(relation.ToEntityId),-24} [{DescribeRelationTags(relation.Tags)}]");
            }

            builder.AppendLine();
            builder.AppendLine("=== Match Result ===");
            builder.AppendLine($"Matched storylets: {result.Matches.Count}");

            foreach (var matchedStorylet in result.Matches)
            {
                builder.AppendLine();
                builder.AppendLine($"  {matchedStorylet.Storylet.Key}");

                foreach (var assignment in matchedStorylet.Assignment)
                {
                    builder.AppendLine($"    {assignment.Role.Key,-20} -> {assignment.Entity.Key}");
                }
            }

            builder.AppendLine();
            builder.AppendLine($"Unmatched storylets: {result.UnmatchedStorylets.Count}");

            foreach (var unmatchedStorylet in result.UnmatchedStorylets)
            {
                builder.AppendLine(
                    $"  {unmatchedStorylet.Storylet.Key}  [reason: {unmatchedStorylet.Reason}]");
            }

            return builder.ToString();
        }

        private static List<Entity> BuildSampleEntities()
        {
            return new List<Entity>
            {
                new(NoblePatronEntityId, "entity.noble_patron", NobleTag | UrbanTag, BuildAttributes((WealthAttributeId, 90f))),
                new(SwornKnightEntityId, "entity.sworn_knight", WarriorTag | UrbanTag),
                new(RivalMercenaryEntityId, "entity.rival_mercenary", WarriorTag | MercenaryTag | ScoutTag),
                new(CityGuardEntityId, "entity.city_guard", WarriorTag | UrbanTag),
                new(ScoutEnvoyEntityId, "entity.scout_envoy", ScoutTag | UrbanTag),
                new(SmugglerEntityId, "entity.smuggler", BanditTag | ScoutTag | UrbanTag, BuildAttributes((WealthAttributeId, 15f))),
                new(CourtPriestEntityId, "entity.court_priest", PriestTag | UrbanTag),
                new(RitualAcolyteEntityId, "entity.ritual_acolyte", PriestTag | RuralTag),
                new(RitualMageEntityId, "entity.ritual_mage", ArcaneTag | UrbanTag),
                new(OutlawScoutEntityId, "entity.outlaw_scout", BanditTag | ScoutTag | OutlawTag | RuralTag)
            };
        }

        private static List<Storylet> BuildSampleStorylets()
        {
            return new List<Storylet>
            {
                new(
                    SwornEscortStoryletId,
                    "storylet.sworn_escort",
                    1.35f,
                    new List<Role>
                    {
                        new(
                            NobleRoleId,
                            "role.noble",
                            QueryWithAllOf(NobleTag),
                            new[]
                            {
                                AttributeRequirement.Min(WealthAttributeId, 20f)
                            },
                            new[]
                            {
                                new AttributePreference(WealthAttributeId, 1f, 20f, 100f)
                            }),
                        new(
                            KnightRoleId,
                            "role.knight",
                            QueryWithAllOf(WarriorTag),
                            relationRequirements: new[]
                            {
                                new RelationRequirement(
                                    NobleRoleId,
                                    new TagQuery(TagSet.Empty, SwornAllyRelationTag | VassalRelationTag, TagSet.Empty),
                                    RelationDirection.FromSelfToTarget)
                            })
                    }),
                new(
                    SecretAudienceStoryletId,
                    "storylet.secret_audience",
                    1.28f,
                    new List<Role>
                    {
                        new(PatronRoleId, "role.patron", QueryWithAllOf(NobleTag)),
                        new(
                            EnvoyRoleId,
                            "role.envoy",
                            QueryWithAllOf(ScoutTag),
                            relationRequirements: new[]
                            {
                                new RelationRequirement(
                                    PatronRoleId,
                                    new TagQuery(TagSet.Empty, TrustRelationTag, TagSet.Empty),
                                    RelationDirection.FromTargetToSelf)
                            })
                    }),
                new(
                    MutualRitualStoryletId,
                    "storylet.mutual_ritual",
                    1.3f,
                    new List<Role>
                    {
                        new(
                            RitualistRoleId,
                            "role.ritualist",
                            QueryWithAllOf(ArcaneTag),
                            relationRequirements: new[]
                            {
                                new RelationRequirement(
                                    WitnessRoleId,
                                    new TagQuery(TagSet.Empty, CovenantRelationTag, TagSet.Empty),
                                    RelationDirection.BothDirections)
                            }),
                        new(WitnessRoleId, "role.witness", QueryWithAllOf(PriestTag))
                    }),
                new(
                    GuardedSermonStoryletId,
                    "storylet.guarded_sermon",
                    1.22f,
                    new List<Role>
                    {
                        new(
                            PriestRoleId,
                            "role.priest",
                            QueryWithAllOf(PriestTag),
                            relationRequirements: new[]
                            {
                                new RelationRequirement(
                                    GuardRoleId,
                                    new TagQuery(TagSet.Empty, TagSet.Empty, EnemyRelationTag),
                                    RelationDirection.AnyDirection)
                            }),
                        new(GuardRoleId, "role.guard", QueryWithAllOf(WarriorTag))
                    }),
                new(
                    CityPatrolStoryletId,
                    "storylet.city_patrol",
                    1.15f,
                    new List<Role>
                    {
                        new(PatrolGuardRoleId, "role.guard", QueryWithAllOf(WarriorTag | UrbanTag))
                    }),
                new(
                    ContrabandDropStoryletId,
                    "storylet.contraband_drop",
                    1.1f,
                    new List<Role>
                    {
                        new(SmugglerRoleId, "role.smuggler", QueryWithAllOf(BanditTag | UrbanTag)),
                        new(
                            LookoutRoleId,
                            "role.lookout",
                            QueryWithAllOf(ScoutTag),
                            relationRequirements: new[]
                            {
                                new RelationRequirement(
                                    SmugglerRoleId,
                                    new TagQuery(TagSet.Empty, TrustRelationTag, EnemyRelationTag),
                                    RelationDirection.AnyDirection)
                            }),
                        new(RunnerRoleId, "role.runner", QueryWithAllOf(ScoutTag))
                    })
            };
        }

        private static List<EntityRelation> BuildSampleRelations()
        {
            return new List<EntityRelation>
            {
                new(SwornKnightEntityId, NoblePatronEntityId, VassalRelationTag),
                new(NoblePatronEntityId, SwornKnightEntityId, SwornAllyRelationTag),
                new(NoblePatronEntityId, ScoutEnvoyEntityId, TrustRelationTag),
                new(RitualMageEntityId, RitualAcolyteEntityId, CovenantRelationTag),
                new(RitualAcolyteEntityId, RitualMageEntityId, CovenantRelationTag),
                new(RivalMercenaryEntityId, CourtPriestEntityId, EnemyRelationTag),
                new(CourtPriestEntityId, RivalMercenaryEntityId, EnemyRelationTag),
                new(SmugglerEntityId, OutlawScoutEntityId, TrustRelationTag),
                new(OutlawScoutEntityId, SmugglerEntityId, TrustRelationTag)
            };
        }

        private static string DescribeEntityId(EntityId entityId)
        {
            return entityId switch
            {
                var id when id == NoblePatronEntityId => "entity.noble_patron",
                var id when id == SwornKnightEntityId => "entity.sworn_knight",
                var id when id == RivalMercenaryEntityId => "entity.rival_mercenary",
                var id when id == CityGuardEntityId => "entity.city_guard",
                var id when id == ScoutEnvoyEntityId => "entity.scout_envoy",
                var id when id == SmugglerEntityId => "entity.smuggler",
                var id when id == CourtPriestEntityId => "entity.court_priest",
                var id when id == RitualAcolyteEntityId => "entity.ritual_acolyte",
                var id when id == RitualMageEntityId => "entity.ritual_mage",
                var id when id == OutlawScoutEntityId => "entity.outlaw_scout",
                _ => entityId.ToString()
            };
        }

        private static TagQuery QueryWithAllOf(
            TagSet allOf,
            TagSet anyOf = default,
            TagSet noneOf = default)
        {
            return new TagQuery(allOf, anyOf, noneOf);
        }

        private static TagQuery QueryWithAnyOf(TagSet tags)
        {
            return new TagQuery(TagSet.Empty, tags, TagSet.Empty);
        }

        private static string DescribeTags(TagSet tags)
        {
            var names = new List<string>();
            AppendTagName(names, tags, WarriorTag, "tag.warrior");
            AppendTagName(names, tags, BanditTag, "tag.bandit");
            AppendTagName(names, tags, PriestTag, "tag.priest");
            AppendTagName(names, tags, ScoutTag, "tag.scout");
            AppendTagName(names, tags, NobleTag, "tag.noble");
            AppendTagName(names, tags, OutlawTag, "tag.outlaw");
            AppendTagName(names, tags, RuralTag, "tag.rural");
            AppendTagName(names, tags, UrbanTag, "tag.urban");
            AppendTagName(names, tags, ArcaneTag, "tag.arcane");
            AppendTagName(names, tags, MercenaryTag, "tag.mercenary");
            return names.Count == 0 ? "None" : string.Join(", ", names);
        }

        private static string DescribeRelationTags(TagSet tags)
        {
            var names = new List<string>();
            AppendTagName(names, tags, SwornAllyRelationTag, "tag.sworn_ally");
            AppendTagName(names, tags, VassalRelationTag, "tag.vassal");
            AppendTagName(names, tags, TrustRelationTag, "tag.trust");
            AppendTagName(names, tags, CovenantRelationTag, "tag.covenant");
            AppendTagName(names, tags, EnemyRelationTag, "tag.enemy");
            return names.Count == 0 ? "None" : string.Join(", ", names);
        }

        private static string DescribeAttributes(AttributeSet attributes)
        {
            var parts = new List<string>();

            if (attributes.TryGet(WealthAttributeId, out var wealth))
            {
                parts.Add($"Wealth={wealth:0.##}");
            }

            return parts.Count == 0 ? "No attributes" : string.Join(", ", parts);
        }

        private static AttributeSet BuildAttributes(params (AttributeId Id, float Value)[] pairs)
        {
            var maxIndex = -1;

            foreach (var pair in pairs)
            {
                if (pair.Id.Value > maxIndex)
                {
                    maxIndex = pair.Id.Value;
                }
            }

            var values = maxIndex >= 0
                ? new float[maxIndex + 1]
                : System.Array.Empty<float>();

            foreach (var pair in pairs)
            {
                values[pair.Id.Value] = pair.Value;
            }

            return new AttributeSet(values);
        }

        private static void AppendTagName(
            List<string> names,
            TagSet value,
            TagSet tag,
            string name)
        {
            if (value.ContainsAll(tag))
            {
                names.Add(name);
            }
        }

        public void Start()
        {
            LogSample();
        }
    }
}
