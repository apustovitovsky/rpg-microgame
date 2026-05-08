using System.Collections.Generic;

namespace Etheria.Features.StoryletSystem
{
    public static class StoryletSmokeStorylets
    {
        public static IReadOnlyList<StoryletDefinition> Build(StoryletSmokeCompilerContext ctx)
        {
            return new List<StoryletDefinition>
            {
                BuildBorderWarning(ctx),
                BuildCallToArms(ctx),
                BuildArcaneRitual(ctx),
                BuildBanditRaid(ctx),
                BuildHealTheWounded(ctx),
                BuildSearchForMissingChild(ctx),
                BuildBanishTheRaider(ctx)
            };
        }

        private static StoryletDefinition BuildBorderWarning(StoryletSmokeCompilerContext ctx)
        {
            return ctx.Storylet(
                StoryletSmokeSymbols.Storylets.BorderWarning,
                2.5f,
                new List<Role>
                {
                    ctx.Role(StoryletSmokeSymbols.Roles.ScoutReporter, ctx.Query(allOf: new[] { StoryletSmokeSymbols.Tags.Bandit })),
                    ctx.Role(StoryletSmokeSymbols.Roles.Authority, ctx.Query(anyOf: new[] { StoryletSmokeSymbols.Tags.Noble, StoryletSmokeSymbols.Tags.Settler }))
                },
                new IStoryletPrecondition[]
                {
                    ctx.EntityTagExists(ctx.Query(allOf: new[] { StoryletSmokeSymbols.Tags.Bandit }), "A bandit scout must exist."),
                    ctx.EntityTagExists(ctx.Query(anyOf: new[] { StoryletSmokeSymbols.Tags.Noble, StoryletSmokeSymbols.Tags.Settler }), "An authority figure must exist.")
                },
                new StoryletEffect[]
                {
                    ctx.AddEntityTag(StoryletSmokeSymbols.Roles.Authority, StoryletSmokeSymbols.Tags.Alerted),
                    ctx.AddEntityAttribute(StoryletSmokeSymbols.Roles.Authority, StoryletSmokeSymbols.Attributes.Fear, 15f),
                    ctx.AddWorldAttribute(StoryletSmokeSymbols.Attributes.Fear, 15f)
                },
                StoryletRepeatabilityPolicy.OncePerRun(),
                new StoryletSaliencePolicy(10f, 5f, 4f, 1f));
        }

        private static StoryletDefinition BuildCallToArms(StoryletSmokeCompilerContext ctx)
        {
            return ctx.Storylet(
                StoryletSmokeSymbols.Storylets.CallToArms,
                3f,
                new List<Role>
                {
                    ctx.Role(StoryletSmokeSymbols.Roles.Commander, ctx.Query(anyOf: new[] { StoryletSmokeSymbols.Tags.Noble, StoryletSmokeSymbols.Tags.Warrior })),
                    ctx.Role(
                        StoryletSmokeSymbols.Roles.Guard,
                        ctx.Query(allOf: new[] { StoryletSmokeSymbols.Tags.Warrior }),
                        relationRequirements: new[]
                        {
                            ctx.RelationRequirement(
                                StoryletSmokeSymbols.Roles.Commander,
                                ctx.Query(anyOf: new[] { StoryletSmokeSymbols.Tags.TrustRelation, StoryletSmokeSymbols.Tags.VassalRelation }),
                                RelationDirection.FromSelfToTarget)
                        })
                },
                new IStoryletPrecondition[]
                {
                    ctx.EntityTagExists(ctx.Query(allOf: new[] { StoryletSmokeSymbols.Tags.Alerted }), "An alerted authority is required.")
                },
                new StoryletEffect[]
                {
                    ctx.AddEntityTag(StoryletSmokeSymbols.Roles.Guard, StoryletSmokeSymbols.Tags.Mobilized),
                    ctx.AddEntityTag(StoryletSmokeSymbols.Roles.Commander, StoryletSmokeSymbols.Tags.LeadingDefense),
                    ctx.AddRelationTag(StoryletSmokeSymbols.Roles.Guard, StoryletSmokeSymbols.Roles.Commander, StoryletSmokeSymbols.Tags.SwornToRelation)
                },
                StoryletRepeatabilityPolicy.OncePerRun(),
                new StoryletSaliencePolicy(8f, 4f, 4f, 1f));
        }

        private static StoryletDefinition BuildArcaneRitual(StoryletSmokeCompilerContext ctx)
        {
            return ctx.Storylet(
                StoryletSmokeSymbols.Storylets.ArcaneRitual,
                3.4f,
                new List<Role>
                {
                    ctx.Role(
                        StoryletSmokeSymbols.Roles.Mage,
                        ctx.Query(allOf: new[] { StoryletSmokeSymbols.Tags.Arcane }),
                        relationRequirements: new[]
                        {
                            ctx.RelationRequirement(
                                StoryletSmokeSymbols.Roles.Acolyte,
                                ctx.Query(anyOf: new[] { StoryletSmokeSymbols.Tags.TrustRelation }, noneOf: new[] { StoryletSmokeSymbols.Tags.EnemyRelation }),
                                RelationDirection.AnyDirection)
                        }),
                    ctx.Role(StoryletSmokeSymbols.Roles.Acolyte, ctx.Query(allOf: new[] { StoryletSmokeSymbols.Tags.Priest }))
                },
                new IStoryletPrecondition[]
                {
                    ctx.EntityTagExists(ctx.Query(allOf: new[] { StoryletSmokeSymbols.Tags.Alerted }), "The settlement must already be alerted."),
                    ctx.EntityTagExists(ctx.Query(allOf: new[] { StoryletSmokeSymbols.Tags.LeadingDefense }), "The settlement must have committed to a defense plan."),
                    ctx.EntityMissing(StoryletSmokeSymbols.Entities.WardManifestation, "The ward must not already exist.")
                },
                new StoryletEffect[]
                {
                    ctx.AddEntityTag(StoryletSmokeSymbols.Roles.Mage, StoryletSmokeSymbols.Tags.Exhausted),
                    ctx.SpawnEntity(StoryletSmokeSymbols.Entities.WardManifestation, ctx.Tags(StoryletSmokeSymbols.Tags.Ward),
                        (StoryletSmokeSymbols.Attributes.Health, 100f)),
                    ctx.AddWorldAttribute(StoryletSmokeSymbols.Attributes.WardStrength, 50f)
                },
                StoryletRepeatabilityPolicy.OncePerRun(),
                new StoryletSaliencePolicy(9f, 4f, 4f, 1f));
        }

        private static StoryletDefinition BuildBanditRaid(StoryletSmokeCompilerContext ctx)
        {
            return ctx.Storylet(
                StoryletSmokeSymbols.Storylets.BanditRaid,
                3.1f,
                new List<Role>
                {
                    ctx.Role(StoryletSmokeSymbols.Roles.Raider, ctx.Query(allOf: new[] { StoryletSmokeSymbols.Tags.Bandit })),
                    ctx.Role(StoryletSmokeSymbols.Roles.Defender, ctx.Query(allOf: new[] { StoryletSmokeSymbols.Tags.Warrior })),
                    ctx.Role(StoryletSmokeSymbols.Roles.Guardian, ctx.Query(allOf: new[] { StoryletSmokeSymbols.Tags.Child }))
                },
                new IStoryletPrecondition[]
                {
                    ctx.EntityTagExists(ctx.Query(allOf: new[] { StoryletSmokeSymbols.Tags.Alerted }), "The settlement must already be alerted."),
                    ctx.EntityTagExists(ctx.Query(allOf: new[] { StoryletSmokeSymbols.Tags.LeadingDefense }), "The settlement must have committed to a defense plan."),
                    ctx.EntityMissing(StoryletSmokeSymbols.Entities.WardManifestation, "The ritual ward blocks the raid.")
                },
                new StoryletEffect[]
                {
                    ctx.AddEntityTag(StoryletSmokeSymbols.Roles.Defender, StoryletSmokeSymbols.Tags.Wounded),
                    ctx.AddEntityAttribute(StoryletSmokeSymbols.Roles.Defender, StoryletSmokeSymbols.Attributes.Health, -20f),
                    ctx.AddEntityTag(StoryletSmokeSymbols.Roles.Guardian, StoryletSmokeSymbols.Tags.Missing),
                    ctx.AddWorldAttribute(StoryletSmokeSymbols.Attributes.Fear, 20f),
                    ctx.AddEntityTag(StoryletSmokeSymbols.Roles.Raider, StoryletSmokeSymbols.Tags.Outlaw)
                },
                StoryletRepeatabilityPolicy.UntilStateChanges(),
                new StoryletSaliencePolicy(5f, 0f, 20f, 1f));
        }

        private static StoryletDefinition BuildHealTheWounded(StoryletSmokeCompilerContext ctx)
        {
            return ctx.Storylet(
                StoryletSmokeSymbols.Storylets.HealTheWounded,
                2.1f,
                new List<Role>
                {
                    ctx.Role(StoryletSmokeSymbols.Roles.Healer, ctx.Query(allOf: new[] { StoryletSmokeSymbols.Tags.Priest })),
                    ctx.Role(StoryletSmokeSymbols.Roles.Patient, ctx.Query(allOf: new[] { StoryletSmokeSymbols.Tags.Wounded }))
                },
                new IStoryletPrecondition[]
                {
                    ctx.EntityTagExists(ctx.Query(allOf: new[] { StoryletSmokeSymbols.Tags.Wounded }), "A wounded defender must exist.")
                },
                new StoryletEffect[]
                {
                    ctx.RemoveEntityTag(StoryletSmokeSymbols.Roles.Patient, StoryletSmokeSymbols.Tags.Wounded),
                    ctx.AddEntityAttribute(StoryletSmokeSymbols.Roles.Patient, StoryletSmokeSymbols.Attributes.Health, 25f),
                    ctx.AddRelationTag(StoryletSmokeSymbols.Roles.Patient, StoryletSmokeSymbols.Roles.Healer, StoryletSmokeSymbols.Tags.GratitudeRelation)
                },
                StoryletRepeatabilityPolicy.WithCooldown(1),
                new StoryletSaliencePolicy(10f, 5f, 3f, 0.5f));
        }

        private static StoryletDefinition BuildSearchForMissingChild(StoryletSmokeCompilerContext ctx)
        {
            return ctx.Storylet(
                StoryletSmokeSymbols.Storylets.SearchForMissingChild,
                2.4f,
                new List<Role>
                {
                    ctx.Role(StoryletSmokeSymbols.Roles.Searcher, ctx.Query(allOf: new[] { StoryletSmokeSymbols.Tags.Warrior })),
                    ctx.Role(StoryletSmokeSymbols.Roles.Guardian, ctx.Query(allOf: new[] { StoryletSmokeSymbols.Tags.Child, StoryletSmokeSymbols.Tags.Missing }))
                },
                new IStoryletPrecondition[]
                {
                    ctx.EntityTagExists(ctx.Query(allOf: new[] { StoryletSmokeSymbols.Tags.Missing }), "A missing child must exist.")
                },
                new StoryletEffect[]
                {
                    ctx.RemoveEntityTag(StoryletSmokeSymbols.Roles.Guardian, StoryletSmokeSymbols.Tags.Missing),
                    ctx.AddEntityTag(StoryletSmokeSymbols.Roles.Guardian, StoryletSmokeSymbols.Tags.Rescued),
                    ctx.AddRelationTag(StoryletSmokeSymbols.Roles.Guardian, StoryletSmokeSymbols.Roles.Searcher, StoryletSmokeSymbols.Tags.TrustRelation)
                },
                StoryletRepeatabilityPolicy.OncePerRun(),
                new StoryletSaliencePolicy(5f, 1f, 3f, 0.5f));
        }

        private static StoryletDefinition BuildBanishTheRaider(StoryletSmokeCompilerContext ctx)
        {
            return ctx.Storylet(
                StoryletSmokeSymbols.Storylets.BanishTheRaider,
                2.8f,
                new List<Role>
                {
                    ctx.Role(StoryletSmokeSymbols.Roles.Judge, ctx.Query(anyOf: new[] { StoryletSmokeSymbols.Tags.Noble, StoryletSmokeSymbols.Tags.Warrior })),
                    ctx.Role(StoryletSmokeSymbols.Roles.Outlaw, ctx.Query(allOf: new[] { StoryletSmokeSymbols.Tags.Outlaw }))
                },
                new IStoryletPrecondition[]
                {
                    ctx.EntityTagExists(ctx.Query(allOf: new[] { StoryletSmokeSymbols.Tags.Outlaw }), "An outlaw raider must exist.")
                },
                new StoryletEffect[]
                {
                    ctx.DespawnEntity(StoryletSmokeSymbols.Roles.Outlaw)
                },
                StoryletRepeatabilityPolicy.OncePerRun(),
                new StoryletSaliencePolicy(1f, 0f, 3f, 0.5f));
        }
    }
}
