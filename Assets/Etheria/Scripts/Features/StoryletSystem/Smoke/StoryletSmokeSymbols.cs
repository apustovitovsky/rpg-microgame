using System.Collections.Generic;

namespace Etheria.Features.StoryletSystem
{
    public static class StoryletSmokeSymbols
    {
        public static class Tags
        {
            public static readonly TagSymbol Noble = new("tag.noble");
            public static readonly TagSymbol Warrior = new("tag.warrior");
            public static readonly TagSymbol Bandit = new("tag.bandit");
            public static readonly TagSymbol Priest = new("tag.priest");
            public static readonly TagSymbol Arcane = new("tag.arcane");
            public static readonly TagSymbol Settler = new("tag.settler");
            public static readonly TagSymbol Child = new("tag.child");
            public static readonly TagSymbol Wounded = new("tag.wounded");
            public static readonly TagSymbol Missing = new("tag.missing");
            public static readonly TagSymbol Alerted = new("tag.alerted");
            public static readonly TagSymbol Mobilized = new("tag.mobilized");
            public static readonly TagSymbol LeadingDefense = new("tag.leading_defense");
            public static readonly TagSymbol Exhausted = new("tag.exhausted");
            public static readonly TagSymbol Rescued = new("tag.rescued");
            public static readonly TagSymbol Outlaw = new("tag.outlaw");
            public static readonly TagSymbol Ward = new("tag.ward");
            public static readonly TagSymbol VassalRelation = new("tag.rel.vassal");
            public static readonly TagSymbol TrustRelation = new("tag.rel.trust");
            public static readonly TagSymbol EnemyRelation = new("tag.rel.enemy");
            public static readonly TagSymbol CaretakerRelation = new("tag.rel.caretaker");
            public static readonly TagSymbol SwornToRelation = new("tag.rel.sworn_to");
            public static readonly TagSymbol GratitudeRelation = new("tag.rel.gratitude");

            public static IReadOnlyList<TagSymbol> All { get; } = new[]
            {
                Noble, Warrior, Bandit, Priest, Arcane, Settler, Child, Wounded,
                Missing, Alerted, Mobilized, LeadingDefense, Exhausted, Rescued, Outlaw, Ward,
                VassalRelation, TrustRelation, EnemyRelation, CaretakerRelation, SwornToRelation, GratitudeRelation
            };
        }

        public static class Attributes
        {
            public static readonly AttributeSymbol Wealth = new("attr.wealth");
            public static readonly AttributeSymbol Fear = new("attr.fear");
            public static readonly AttributeSymbol Loyalty = new("attr.loyalty");
            public static readonly AttributeSymbol Health = new("attr.health");
            public static readonly AttributeSymbol WardStrength = new("attr.ward_strength");

            public static IReadOnlyList<AttributeSymbol> All { get; } = new[]
            {
                Wealth, Fear, Loyalty, Health, WardStrength
            };
        }

        public static class Entities
        {
            public static readonly EntitySymbol VillageElder = new("entity.village_elder");
            public static readonly EntitySymbol YoungKnight = new("entity.young_knight");
            public static readonly EntitySymbol BanditScout = new("entity.bandit_scout");
            public static readonly EntitySymbol TemplePriest = new("entity.temple_priest");
            public static readonly EntitySymbol WanderingMage = new("entity.wandering_mage");
            public static readonly EntitySymbol MilitiaGuard = new("entity.militia_guard");
            public static readonly EntitySymbol RefugeeChild = new("entity.refugee_child");
            public static readonly EntitySymbol SettlementSquare = new("entity.settlement_square");
            public static readonly EntitySymbol WardManifestation = new("entity.ward_manifestation");

            public static IReadOnlyList<EntitySymbol> All { get; } = new[]
            {
                VillageElder, YoungKnight, BanditScout, TemplePriest, WanderingMage,
                MilitiaGuard, RefugeeChild, SettlementSquare, WardManifestation
            };
        }

        public static class Roles
        {
            public static readonly RoleSymbol ScoutReporter = new("role.scout_reporter");
            public static readonly RoleSymbol Authority = new("role.authority");
            public static readonly RoleSymbol Commander = new("role.commander");
            public static readonly RoleSymbol Guard = new("role.guard");
            public static readonly RoleSymbol Mage = new("role.mage");
            public static readonly RoleSymbol Acolyte = new("role.acolyte");
            public static readonly RoleSymbol Raider = new("role.raider");
            public static readonly RoleSymbol Defender = new("role.defender");
            public static readonly RoleSymbol Healer = new("role.healer");
            public static readonly RoleSymbol Patient = new("role.patient");
            public static readonly RoleSymbol Searcher = new("role.searcher");
            public static readonly RoleSymbol Guardian = new("role.guardian");
            public static readonly RoleSymbol Judge = new("role.judge");
            public static readonly RoleSymbol Outlaw = new("role.outlaw");

            public static IReadOnlyList<RoleSymbol> All { get; } = new[]
            {
                ScoutReporter, Authority, Commander, Guard, Mage, Acolyte, Raider,
                Defender, Healer, Patient, Searcher, Guardian, Judge, Outlaw
            };
        }

        public static class Storylets
        {
            public static readonly StoryletSymbol BorderWarning = new("storylet.border_warning");
            public static readonly StoryletSymbol CallToArms = new("storylet.call_to_arms");
            public static readonly StoryletSymbol ArcaneRitual = new("storylet.arcane_ritual");
            public static readonly StoryletSymbol BanditRaid = new("storylet.bandit_raid");
            public static readonly StoryletSymbol HealTheWounded = new("storylet.heal_the_wounded");
            public static readonly StoryletSymbol SearchForMissingChild = new("storylet.search_for_missing_child");
            public static readonly StoryletSymbol BanishTheRaider = new("storylet.banish_the_raider");

            public static IReadOnlyList<StoryletSymbol> All { get; } = new[]
            {
                BorderWarning, CallToArms, ArcaneRitual, BanditRaid,
                HealTheWounded, SearchForMissingChild, BanishTheRaider
            };
        }
    }

    public interface IStoryletSymbol
    {
        string Key { get; }
    }

    public readonly struct TagSymbol : IStoryletSymbol
    {
        public TagSymbol(string key) => Key = key;
        public string Key { get; }
        public override string ToString() => Key;
    }

    public readonly struct AttributeSymbol : IStoryletSymbol
    {
        public AttributeSymbol(string key) => Key = key;
        public string Key { get; }
        public override string ToString() => Key;
    }

    public readonly struct EntitySymbol : IStoryletSymbol
    {
        public EntitySymbol(string key) => Key = key;
        public string Key { get; }
        public override string ToString() => Key;
    }

    public readonly struct RoleSymbol : IStoryletSymbol
    {
        public RoleSymbol(string key) => Key = key;
        public string Key { get; }
        public override string ToString() => Key;
    }

    public readonly struct StoryletSymbol : IStoryletSymbol
    {
        public StoryletSymbol(string key) => Key = key;
        public string Key { get; }
        public override string ToString() => Key;
    }
}
