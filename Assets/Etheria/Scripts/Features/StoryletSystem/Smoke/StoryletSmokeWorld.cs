using System.Collections.Generic;

namespace Etheria.Features.StoryletSystem
{
    public static class StoryletSmokeWorld
    {
        public static StoryletWorldState Build(StoryletSmokeCompilerContext ctx)
        {
            var entities = new List<Entity>
            {
                ctx.Entity(StoryletSmokeSymbols.Entities.VillageElder, ctx.Tags(StoryletSmokeSymbols.Tags.Noble, StoryletSmokeSymbols.Tags.Settler),
                    (StoryletSmokeSymbols.Attributes.Wealth, 30f),
                    (StoryletSmokeSymbols.Attributes.Fear, 10f),
                    (StoryletSmokeSymbols.Attributes.Loyalty, 60f)),
                ctx.Entity(StoryletSmokeSymbols.Entities.YoungKnight, ctx.Tags(StoryletSmokeSymbols.Tags.Warrior, StoryletSmokeSymbols.Tags.Noble),
                    (StoryletSmokeSymbols.Attributes.Health, 80f),
                    (StoryletSmokeSymbols.Attributes.Loyalty, 70f)),
                ctx.Entity(StoryletSmokeSymbols.Entities.BanditScout, ctx.Tags(StoryletSmokeSymbols.Tags.Bandit),
                    (StoryletSmokeSymbols.Attributes.Health, 55f)),
                ctx.Entity(StoryletSmokeSymbols.Entities.TemplePriest, ctx.Tags(StoryletSmokeSymbols.Tags.Priest, StoryletSmokeSymbols.Tags.Settler),
                    (StoryletSmokeSymbols.Attributes.Health, 65f)),
                ctx.Entity(StoryletSmokeSymbols.Entities.WanderingMage, ctx.Tags(StoryletSmokeSymbols.Tags.Arcane, StoryletSmokeSymbols.Tags.Settler),
                    (StoryletSmokeSymbols.Attributes.Health, 55f)),
                ctx.Entity(StoryletSmokeSymbols.Entities.MilitiaGuard, ctx.Tags(StoryletSmokeSymbols.Tags.Warrior, StoryletSmokeSymbols.Tags.Settler),
                    (StoryletSmokeSymbols.Attributes.Health, 70f),
                    (StoryletSmokeSymbols.Attributes.Loyalty, 50f)),
                ctx.Entity(StoryletSmokeSymbols.Entities.RefugeeChild, ctx.Tags(StoryletSmokeSymbols.Tags.Child, StoryletSmokeSymbols.Tags.Settler),
                    (StoryletSmokeSymbols.Attributes.Health, 45f)),
                ctx.Entity(StoryletSmokeSymbols.Entities.SettlementSquare, ctx.Tags(StoryletSmokeSymbols.Tags.Settler))
            };

            var relations = new List<EntityRelation>
            {
                ctx.Relation(StoryletSmokeSymbols.Entities.YoungKnight, StoryletSmokeSymbols.Entities.VillageElder, ctx.Tags(StoryletSmokeSymbols.Tags.VassalRelation)),
                ctx.Relation(StoryletSmokeSymbols.Entities.VillageElder, StoryletSmokeSymbols.Entities.YoungKnight, ctx.Tags(StoryletSmokeSymbols.Tags.TrustRelation)),
                ctx.Relation(StoryletSmokeSymbols.Entities.BanditScout, StoryletSmokeSymbols.Entities.MilitiaGuard, ctx.Tags(StoryletSmokeSymbols.Tags.EnemyRelation)),
                ctx.Relation(StoryletSmokeSymbols.Entities.TemplePriest, StoryletSmokeSymbols.Entities.RefugeeChild, ctx.Tags(StoryletSmokeSymbols.Tags.CaretakerRelation)),
                ctx.Relation(StoryletSmokeSymbols.Entities.WanderingMage, StoryletSmokeSymbols.Entities.TemplePriest, ctx.Tags(StoryletSmokeSymbols.Tags.TrustRelation))
            };

            return new StoryletWorldState(
                0,
                entities,
                relations,
                ctx.Attributes(
                    (StoryletSmokeSymbols.Attributes.Fear, 10f),
                    (StoryletSmokeSymbols.Attributes.WardStrength, 0f)));
        }
    }
}
