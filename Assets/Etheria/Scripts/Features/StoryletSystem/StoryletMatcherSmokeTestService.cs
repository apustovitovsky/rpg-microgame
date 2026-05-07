using System.Collections.Generic;
using System.Text;
using UnityEngine;
using VContainer.Unity;

namespace Etheria.Features.StoryletSystem
{
    public sealed class StoryletMatcherSmokeTestService : IStartable
    {
        private static readonly AttributeId WealthAttributeId = new(0);

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
                    $"  - {entity.Id,-24} [{DescribeTags(entity.Tags)}] [{DescribeAttributes(entity.Attributes)}]");
            }

            builder.AppendLine();
            builder.AppendLine($"Storylets: {storylets.Count}");

            foreach (var storylet in storylets)
            {
                builder.AppendLine();
                builder.AppendLine($"  {storylet.Id}  (priority {storylet.Priority:0.##})");

                foreach (var role in storylet.Roles)
                {
                    builder.Append($"    {role.Id,-20} <- ");

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

                        builder.Append(entity.Id);
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
                    $"  - {relation.FromEntityId,-24} -> {relation.ToEntityId,-24} [{DescribeRelationTags(relation.Tags)}]");
            }

            builder.AppendLine();
            builder.AppendLine("=== Match Result ===");
            builder.AppendLine($"Matched storylets: {result.Matches.Count}");

            foreach (var matchedStorylet in result.Matches)
            {
                builder.AppendLine();
                builder.AppendLine($"  {matchedStorylet.Storylet.Id}");

                foreach (var assignment in matchedStorylet.Assignment)
                {
                    builder.AppendLine($"    {assignment.Role.Id,-20} -> {assignment.Entity.Id}");
                }
            }

            builder.AppendLine();
            builder.AppendLine($"Unmatched storylets: {result.UnmatchedStorylets.Count}");

            foreach (var unmatchedStorylet in result.UnmatchedStorylets)
            {
                builder.AppendLine(
                    $"  {unmatchedStorylet.Storylet.Id}  [reason: {unmatchedStorylet.Reason}]");
            }

            return builder.ToString();
        }

        private static List<Entity> BuildSampleEntities()
        {
            return new List<Entity>
            {
                new("entity.noble_patron", NobleTag | UrbanTag, BuildAttributes((WealthAttributeId, 90f))),
                new("entity.sworn_knight", WarriorTag | UrbanTag),
                new("entity.rival_mercenary", WarriorTag | MercenaryTag | ScoutTag),
                new("entity.city_guard", WarriorTag | UrbanTag),
                new("entity.scout_envoy", ScoutTag | UrbanTag),
                new("entity.smuggler", BanditTag | ScoutTag | UrbanTag, BuildAttributes((WealthAttributeId, 15f))),
                new("entity.court_priest", PriestTag | UrbanTag),
                new("entity.ritual_acolyte", PriestTag | RuralTag),
                new("entity.ritual_mage", ArcaneTag | UrbanTag),
                new("entity.outlaw_scout", BanditTag | ScoutTag | OutlawTag | RuralTag)
            };
        }

        private static List<Storylet> BuildSampleStorylets()
        {
            return new List<Storylet>
            {
                new(
                    "storylet.sworn_escort",
                    1.35f,
                    new List<Role>
                    {
                        new(
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
                            "role.knight",
                            QueryWithAllOf(WarriorTag),
                            relationRequirements: new[]
                            {
                                new RelationRequirement(
                                    "role.noble",
                                    new TagQuery(TagSet.Empty, SwornAllyRelationTag | VassalRelationTag, TagSet.Empty),
                                    RelationDirection.FromSelfToTarget)
                            })
                    }),
                new(
                    "storylet.secret_audience",
                    1.28f,
                    new List<Role>
                    {
                        new("role.patron", QueryWithAllOf(NobleTag)),
                        new(
                            "role.envoy",
                            QueryWithAllOf(ScoutTag),
                            relationRequirements: new[]
                            {
                                new RelationRequirement(
                                    "role.patron",
                                    new TagQuery(TagSet.Empty, TrustRelationTag, TagSet.Empty),
                                    RelationDirection.FromTargetToSelf)
                            })
                    }),
                new(
                    "storylet.mutual_ritual",
                    1.3f,
                    new List<Role>
                    {
                        new(
                            "role.ritualist",
                            QueryWithAllOf(ArcaneTag),
                            relationRequirements: new[]
                            {
                                new RelationRequirement(
                                    "role.witness",
                                    new TagQuery(TagSet.Empty, CovenantRelationTag, TagSet.Empty),
                                    RelationDirection.BothDirections)
                            }),
                        new("role.witness", QueryWithAllOf(PriestTag))
                    }),
                new(
                    "storylet.guarded_sermon",
                    1.22f,
                    new List<Role>
                    {
                        new(
                            "role.priest",
                            QueryWithAllOf(PriestTag),
                            relationRequirements: new[]
                            {
                                new RelationRequirement(
                                    "role.guard",
                                    new TagQuery(TagSet.Empty, TagSet.Empty, EnemyRelationTag),
                                    RelationDirection.AnyDirection)
                            }),
                        new("role.guard", QueryWithAllOf(WarriorTag))
                    }),
                new(
                    "storylet.city_patrol",
                    1.15f,
                    new List<Role>
                    {
                        new("role.guard", QueryWithAllOf(WarriorTag | UrbanTag))
                    }),
                new(
                    "storylet.contraband_drop",
                    1.1f,
                    new List<Role>
                    {
                        new("role.smuggler", QueryWithAllOf(BanditTag | UrbanTag)),
                        new(
                            "role.lookout",
                            QueryWithAllOf(ScoutTag),
                            relationRequirements: new[]
                            {
                                new RelationRequirement(
                                    "role.smuggler",
                                    new TagQuery(TagSet.Empty, TrustRelationTag, EnemyRelationTag),
                                    RelationDirection.AnyDirection)
                            }),
                        new("role.runner", QueryWithAllOf(ScoutTag))
                    })
            };
        }

        private static List<EntityRelation> BuildSampleRelations()
        {
            return new List<EntityRelation>
            {
                new("entity.sworn_knight", "entity.noble_patron", VassalRelationTag),
                new("entity.noble_patron", "entity.sworn_knight", SwornAllyRelationTag),
                new("entity.noble_patron", "entity.scout_envoy", TrustRelationTag),
                new("entity.ritual_mage", "entity.ritual_acolyte", CovenantRelationTag),
                new("entity.ritual_acolyte", "entity.ritual_mage", CovenantRelationTag),
                new("entity.rival_mercenary", "entity.court_priest", EnemyRelationTag),
                new("entity.court_priest", "entity.rival_mercenary", EnemyRelationTag),
                new("entity.smuggler", "entity.outlaw_scout", TrustRelationTag),
                new("entity.outlaw_scout", "entity.smuggler", TrustRelationTag)
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
            AppendTagName(names, tags, WarriorTag, "Warrior");
            AppendTagName(names, tags, BanditTag, "Bandit");
            AppendTagName(names, tags, PriestTag, "Priest");
            AppendTagName(names, tags, ScoutTag, "Scout");
            AppendTagName(names, tags, NobleTag, "Noble");
            AppendTagName(names, tags, OutlawTag, "Outlaw");
            AppendTagName(names, tags, RuralTag, "Rural");
            AppendTagName(names, tags, UrbanTag, "Urban");
            AppendTagName(names, tags, ArcaneTag, "Arcane");
            AppendTagName(names, tags, MercenaryTag, "Mercenary");
            return names.Count == 0 ? "None" : string.Join(", ", names);
        }

        private static string DescribeRelationTags(TagSet tags)
        {
            var names = new List<string>();
            AppendTagName(names, tags, SwornAllyRelationTag, "SwornAlly");
            AppendTagName(names, tags, VassalRelationTag, "Vassal");
            AppendTagName(names, tags, TrustRelationTag, "Trust");
            AppendTagName(names, tags, CovenantRelationTag, "Covenant");
            AppendTagName(names, tags, EnemyRelationTag, "Enemy");
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
