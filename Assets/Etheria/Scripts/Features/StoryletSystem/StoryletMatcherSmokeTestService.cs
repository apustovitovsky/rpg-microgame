using System.Collections.Generic;
using System.Text;
using UnityEngine;
using VContainer.Unity;

namespace Etheria.Features.StoryletSystem
{
    public sealed class StoryletMatcherSmokeTestService : IStartable
    {
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
                builder.AppendLine($"  - {entity.Id,-24} [{DescribeTags(entity.Tags)}]");
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
                new("entity.hedge_knight", WarriorTag | NobleTag | RuralTag),
                new("entity.mercenary", WarriorTag | MercenaryTag | ScoutTag),
                new("entity.outlaw_scout", BanditTag | ScoutTag | OutlawTag | RuralTag),
                new("entity.bandit_raider", BanditTag | OutlawTag | RuralTag),
                new("entity.smuggler", BanditTag | UrbanTag | ScoutTag),
                new("entity.village_priest", PriestTag | RuralTag),
                new("entity.court_mage", ArcaneTag | NobleTag | UrbanTag),
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
                        new("role.noble", QueryWithAllOf(NobleTag)),
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
