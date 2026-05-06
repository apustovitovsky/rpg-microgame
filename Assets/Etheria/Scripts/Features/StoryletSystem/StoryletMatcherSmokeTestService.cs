using System.Collections.Generic;
using System.Text;
using UnityEngine;
using VContainer.Unity;

namespace Etheria.Features.StoryletSystem
{
    public sealed class StoryletMatcherSmokeTestService : IStartable
    {
        private static readonly AttributeId WealthAttributeId = new(0);

        private static readonly TagSet WarriorTag = TagSet.FromId(new TagId(0));
        private static readonly TagSet BanditTag = TagSet.FromId(new TagId(1));
        private static readonly TagSet PriestTag = TagSet.FromId(new TagId(2));
        private static readonly TagSet ScoutTag = TagSet.FromId(new TagId(3));
        private static readonly TagSet NobleTag = TagSet.FromId(new TagId(4));
        private static readonly TagSet OutlawTag = TagSet.FromId(new TagId(5));
        private static readonly TagSet RuralTag = TagSet.FromId(new TagId(6));
        private static readonly TagSet UrbanTag = TagSet.FromId(new TagId(7));
        private static readonly TagSet ArcaneTag = TagSet.FromId(new TagId(8));
        private static readonly TagSet MercenaryTag = TagSet.FromId(new TagId(9));

        private readonly GreedyStoryletMatcher _matcher;

        public StoryletMatcherSmokeTestService(GreedyStoryletMatcher matcher)
        {
            _matcher = matcher;
        }

        public StoryletMatchResult RunSample()
        {
            var entities = BuildSampleEntities();
            var storylets = BuildSampleStorylets();
            return _matcher.Match(entities, storylets);
        }

        public void LogSample()
        {
            var entities = BuildSampleEntities();
            var storylets = BuildSampleStorylets();
            var result = _matcher.Match(entities, storylets);
            Debug.Log(Describe(entities, storylets, result));
        }

        public string Describe(
            IReadOnlyList<Entity> entities,
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

            return builder.ToString();
        }

        private static List<Entity> BuildSampleEntities()
        {
            return new List<Entity>
            {
                new("entity.city_guard", WarriorTag | UrbanTag),
                new("entity.temple_guard", WarriorTag | PriestTag | UrbanTag),
                new(
                    "entity.hedge_knight",
                    WarriorTag | NobleTag | RuralTag,
                    BuildAttributes((WealthAttributeId, 35f))),
                new("entity.mercenary", WarriorTag | MercenaryTag | ScoutTag),
                new("entity.outlaw_scout", BanditTag | ScoutTag | OutlawTag | RuralTag),
                new("entity.bandit_raider", BanditTag | OutlawTag | RuralTag),
                new(
                    "entity.smuggler",
                    BanditTag | UrbanTag | ScoutTag,
                    BuildAttributes((WealthAttributeId, 15f))),
                new("entity.village_priest", PriestTag | RuralTag),
                new(
                    "entity.court_mage",
                    ArcaneTag | NobleTag | UrbanTag,
                    BuildAttributes((WealthAttributeId, 90f))),
                new("entity.wandering_mystic", ArcaneTag | PriestTag | RuralTag)
            };
        }

        private static List<Storylet> BuildSampleStorylets()
        {
            return new List<Storylet>
            {
                new(
                    "storylet.city_patrol",
                    1.2f,
                    new List<Role>
                    {
                        new("role.guard", QueryWithAllOf(WarriorTag | UrbanTag))
                    }),
                new(
                    "storylet.farm_raid",
                    1.1f,
                    new List<Role>
                    {
                        new("role.raider", QueryWithAllOf(BanditTag | RuralTag))
                    }),
                new(
                    "storylet.dark_ritual",
                    1.5f,
                    new List<Role>
                    {
                        new("role.ritual_leader", QueryWithAllOf(ArcaneTag)),
                        new("role.ritual_acolyte", QueryWithAllOf(PriestTag))
                    }),
                new(
                    "storylet.noble_escort",
                    1.3f,
                    new List<Role>
                    {
                        new(
                            "role.noble",
                            QueryWithAllOf(NobleTag),
                            new[]
                            {
                                AttributeRequirement.Min(WealthAttributeId, 0f)
                            }),
                        new("role.bodyguard", QueryWithAllOf(WarriorTag))
                    }),
                new(
                    "storylet.smuggling_run",
                    1.25f,
                    new List<Role>
                    {
                        new("role.smuggler", QueryWithAllOf(BanditTag | UrbanTag)),
                        new("role.lookout", QueryWithAllOf(ScoutTag))
                    }),
                new(
                    "storylet.border_skirmish",
                    1.4f,
                    new List<Role>
                    {
                        new("role.frontliner", QueryWithAllOf(WarriorTag)),
                        new("role.flanker", QueryWithAllOf(ScoutTag)),
                        new("role.chaplain", QueryWithAllOf(PriestTag))
                    }),
                new(
                    "storylet.heresy_inquiry",
                    1.15f,
                    new List<Role>
                    {
                        new("role.inquisitor", QueryWithAllOf(PriestTag, ArcaneTag | UrbanTag)),
                        new("role.witness", QueryWithAnyOf(RuralTag | UrbanTag))
                    })
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
