using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;

namespace Etheria.Features.StoryletSystem
{
    public sealed class StoryletMatcherSmokeTestService : IStartable
    {
        private static readonly AttributeId WealthAttributeId = new(0);
        private static readonly AttributeId FearAttributeId = new(1);
        private static readonly AttributeId LoyaltyAttributeId = new(2);
        private static readonly AttributeId HealthAttributeId = new(3);
        private static readonly AttributeId WardStrengthAttributeId = new(4);

        private static readonly StoryletId BorderWarningStoryletId = new(100);
        private static readonly StoryletId CallToArmsStoryletId = new(101);
        private static readonly StoryletId ArcaneRitualStoryletId = new(102);
        private static readonly StoryletId BanditRaidStoryletId = new(103);
        private static readonly StoryletId HealTheWoundedStoryletId = new(104);
        private static readonly StoryletId SearchForMissingChildStoryletId = new(105);
        private static readonly StoryletId BanishTheRaiderStoryletId = new(106);

        private static readonly RoleId ScoutReporterRoleId = new(100);
        private static readonly RoleId AuthorityRoleId = new(101);
        private static readonly RoleId CommanderRoleId = new(102);
        private static readonly RoleId GuardRoleId = new(103);
        private static readonly RoleId MageRoleId = new(104);
        private static readonly RoleId AcolyteRoleId = new(105);
        private static readonly RoleId RaiderRoleId = new(106);
        private static readonly RoleId DefenderRoleId = new(107);
        private static readonly RoleId HealerRoleId = new(108);
        private static readonly RoleId PatientRoleId = new(109);
        private static readonly RoleId SearcherRoleId = new(110);
        private static readonly RoleId GuardianRoleId = new(111);
        private static readonly RoleId JudgeRoleId = new(112);
        private static readonly RoleId OutlawRoleId = new(113);

        private static readonly EntityId VillageElderEntityId = new(100);
        private static readonly EntityId YoungKnightEntityId = new(101);
        private static readonly EntityId BanditScoutEntityId = new(102);
        private static readonly EntityId TemplePriestEntityId = new(103);
        private static readonly EntityId WanderingMageEntityId = new(104);
        private static readonly EntityId MilitiaGuardEntityId = new(105);
        private static readonly EntityId RefugeeChildEntityId = new(106);
        private static readonly EntityId SettlementSquareEntityId = new(107);
        private static readonly EntityId WardManifestationEntityId = new(108);

        private static readonly TagSet NobleTag = TagSet.FromIndex(0);
        private static readonly TagSet WarriorTag = TagSet.FromIndex(1);
        private static readonly TagSet BanditTag = TagSet.FromIndex(2);
        private static readonly TagSet PriestTag = TagSet.FromIndex(3);
        private static readonly TagSet ArcaneTag = TagSet.FromIndex(4);
        private static readonly TagSet SettlerTag = TagSet.FromIndex(5);
        private static readonly TagSet ChildTag = TagSet.FromIndex(6);
        private static readonly TagSet WoundedTag = TagSet.FromIndex(7);
        private static readonly TagSet MissingTag = TagSet.FromIndex(8);
        private static readonly TagSet AlertedTag = TagSet.FromIndex(9);
        private static readonly TagSet MobilizedTag = TagSet.FromIndex(10);
        private static readonly TagSet LeadingDefenseTag = TagSet.FromIndex(11);
        private static readonly TagSet ExhaustedTag = TagSet.FromIndex(12);
        private static readonly TagSet RescuedTag = TagSet.FromIndex(13);
        private static readonly TagSet OutlawTag = TagSet.FromIndex(14);
        private static readonly TagSet WardTag = TagSet.FromIndex(15);

        private static readonly TagSet VassalRelationTag = TagSet.FromIndex(100);
        private static readonly TagSet TrustRelationTag = TagSet.FromIndex(101);
        private static readonly TagSet EnemyRelationTag = TagSet.FromIndex(102);
        private static readonly TagSet CaretakerRelationTag = TagSet.FromIndex(103);
        private static readonly TagSet SwornToRelationTag = TagSet.FromIndex(104);
        private static readonly TagSet GratitudeRelationTag = TagSet.FromIndex(105);

        private readonly IStoryletPlanner _planner;
        private readonly IStoryletTelemetryFormatter _telemetryFormatter;

        public StoryletMatcherSmokeTestService(
            IStoryletPlanner planner,
            IStoryletTelemetryFormatter telemetryFormatter)
        {
            _planner = planner;
            _telemetryFormatter = telemetryFormatter;
        }

        public void Start()
        {
            var result = _planner.Plan(BuildInitialWorldState(), BuildStorylets(), StoryletPlannerMemory.Empty);
            Debug.Log(_telemetryFormatter.Format(result));
        }

        private static StoryletWorldState BuildInitialWorldState()
        {
            var entities = new List<Entity>
            {
                new(VillageElderEntityId, "entity.village_elder", NobleTag | SettlerTag, BuildAttributes((WealthAttributeId, 30f), (FearAttributeId, 10f), (LoyaltyAttributeId, 60f))),
                new(YoungKnightEntityId, "entity.young_knight", WarriorTag | NobleTag, BuildAttributes((HealthAttributeId, 80f), (LoyaltyAttributeId, 70f))),
                new(BanditScoutEntityId, "entity.bandit_scout", BanditTag, BuildAttributes((HealthAttributeId, 55f))),
                new(TemplePriestEntityId, "entity.temple_priest", PriestTag | SettlerTag, BuildAttributes((HealthAttributeId, 65f))),
                new(WanderingMageEntityId, "entity.wandering_mage", ArcaneTag | SettlerTag, BuildAttributes((HealthAttributeId, 55f))),
                new(MilitiaGuardEntityId, "entity.militia_guard", WarriorTag | SettlerTag, BuildAttributes((HealthAttributeId, 70f), (LoyaltyAttributeId, 50f))),
                new(RefugeeChildEntityId, "entity.refugee_child", ChildTag | SettlerTag, BuildAttributes((HealthAttributeId, 45f))),
                new(SettlementSquareEntityId, "entity.settlement_square", SettlerTag)
            };

            var relations = new List<EntityRelation>
            {
                new(YoungKnightEntityId, VillageElderEntityId, VassalRelationTag),
                new(VillageElderEntityId, YoungKnightEntityId, TrustRelationTag),
                new(BanditScoutEntityId, MilitiaGuardEntityId, EnemyRelationTag),
                new(TemplePriestEntityId, RefugeeChildEntityId, CaretakerRelationTag),
                new(WanderingMageEntityId, TemplePriestEntityId, TrustRelationTag)
            };

            return new StoryletWorldState(
                snapshotId: 0,
                entities,
                relations,
                BuildAttributes((FearAttributeId, 10f), (WardStrengthAttributeId, 0f)));
        }

        private static List<StoryletDefinition> BuildStorylets()
        {
            return new List<StoryletDefinition>
            {
                new(
                    BorderWarningStoryletId,
                    "storylet.border_warning",
                    2.5f,
                    new List<Role>
                    {
                        new(ScoutReporterRoleId, "role.scout_reporter", QueryWithAllOf(BanditTag)),
                        new(AuthorityRoleId, "role.authority", QueryWithAnyOf(NobleTag | SettlerTag))
                    },
                    new IStoryletPrecondition[]
                    {
                        new EntityTagPrecondition(QueryWithAllOf(BanditTag), mustExist: true, "A bandit scout must exist."),
                        new EntityTagPrecondition(QueryWithAnyOf(NobleTag | SettlerTag), mustExist: true, "An authority figure must exist.")
                    },
                    new StoryletEffectBatch(new StoryletEffect[]
                    {
                        new AddEntityTagEffect(AuthorityRoleId, AlertedTag),
                        new AddEntityAttributeEffect(AuthorityRoleId, FearAttributeId, 15f),
                        new AddWorldAttributeEffect(FearAttributeId, 15f)
                    }),
                    StoryletRepeatabilityPolicy.OncePerRun(),
                    new StoryletSaliencePolicy(10f, 5f, 4f, 1f)),

                new(
                    CallToArmsStoryletId,
                    "storylet.call_to_arms",
                    3f,
                    new List<Role>
                    {
                        new(CommanderRoleId, "role.commander", QueryWithAnyOf(NobleTag | WarriorTag)),
                        new(
                            GuardRoleId,
                            "role.guard",
                            QueryWithAllOf(WarriorTag),
                            relationRequirements: new[]
                            {
                                new RelationRequirement(
                                    CommanderRoleId,
                                    new TagQuery(TagSet.Empty, TrustRelationTag | VassalRelationTag, TagSet.Empty),
                                    RelationDirection.FromSelfToTarget)
                            })
                    },
                    new IStoryletPrecondition[]
                    {
                        new EntityTagPrecondition(QueryWithAllOf(AlertedTag), mustExist: true, "An alerted authority is required.")
                    },
                    new StoryletEffectBatch(new StoryletEffect[]
                    {
                        new AddEntityTagEffect(GuardRoleId, MobilizedTag),
                        new AddEntityTagEffect(CommanderRoleId, LeadingDefenseTag),
                        new AddRelationTagEffect(GuardRoleId, CommanderRoleId, SwornToRelationTag)
                    }),
                    StoryletRepeatabilityPolicy.OncePerRun(),
                    new StoryletSaliencePolicy(8f, 4f, 4f, 1f)),

                new(
                    ArcaneRitualStoryletId,
                    "storylet.arcane_ritual",
                    3.4f,
                    new List<Role>
                    {
                        new(
                            MageRoleId,
                            "role.mage",
                            QueryWithAllOf(ArcaneTag),
                            relationRequirements: new[]
                            {
                                new RelationRequirement(
                                    AcolyteRoleId,
                                    new TagQuery(TagSet.Empty, TrustRelationTag, EnemyRelationTag),
                                    RelationDirection.AnyDirection)
                            }),
                        new(AcolyteRoleId, "role.acolyte", QueryWithAllOf(PriestTag))
                    },
                    new IStoryletPrecondition[]
                    {
                        new EntityTagPrecondition(QueryWithAllOf(AlertedTag), mustExist: true, "The settlement must already be alerted."),
                        new EntityTagPrecondition(QueryWithAllOf(LeadingDefenseTag), mustExist: true, "The settlement must have committed to a defense plan."),
                        new EntityExistencePrecondition(WardManifestationEntityId, mustExist: false, "The ward must not already exist.")
                    },
                    new StoryletEffectBatch(new StoryletEffect[]
                    {
                        new AddEntityTagEffect(MageRoleId, ExhaustedTag),
                        new SpawnEntityEffect(new Entity(WardManifestationEntityId, "entity.ward_manifestation", WardTag, BuildAttributes((HealthAttributeId, 100f)))),
                        new AddWorldAttributeEffect(WardStrengthAttributeId, 50f)
                    }),
                    StoryletRepeatabilityPolicy.OncePerRun(),
                    new StoryletSaliencePolicy(9f, 4f, 4f, 1f)),

                new(
                    BanditRaidStoryletId,
                    "storylet.bandit_raid",
                    3.1f,
                    new List<Role>
                    {
                        new(RaiderRoleId, "role.raider", QueryWithAllOf(BanditTag)),
                        new(DefenderRoleId, "role.defender", QueryWithAllOf(WarriorTag)),
                        new(GuardianRoleId, "role.guardian", QueryWithAllOf(ChildTag))
                    },
                    new IStoryletPrecondition[]
                    {
                        new EntityTagPrecondition(QueryWithAllOf(AlertedTag), mustExist: true, "The settlement must already be alerted."),
                        new EntityTagPrecondition(QueryWithAllOf(LeadingDefenseTag), mustExist: true, "The settlement must have committed to a defense plan."),
                        new EntityExistencePrecondition(WardManifestationEntityId, mustExist: false, "The ritual ward blocks the raid.")
                    },
                    new StoryletEffectBatch(new StoryletEffect[]
                    {
                        new AddEntityTagEffect(DefenderRoleId, WoundedTag),
                        new AddEntityAttributeEffect(DefenderRoleId, HealthAttributeId, -20f),
                        new AddEntityTagEffect(GuardianRoleId, MissingTag),
                        new AddWorldAttributeEffect(FearAttributeId, 20f),
                        new AddEntityTagEffect(RaiderRoleId, OutlawTag)
                    }),
                    StoryletRepeatabilityPolicy.UntilStateChanges(),
                    new StoryletSaliencePolicy(5f, 0f, 20f, 1f)),

                new(
                    HealTheWoundedStoryletId,
                    "storylet.heal_the_wounded",
                    2.1f,
                    new List<Role>
                    {
                        new(HealerRoleId, "role.healer", QueryWithAllOf(PriestTag)),
                        new(PatientRoleId, "role.patient", QueryWithAllOf(WoundedTag))
                    },
                    new IStoryletPrecondition[]
                    {
                        new EntityTagPrecondition(QueryWithAllOf(WoundedTag), mustExist: true, "A wounded defender must exist.")
                    },
                    new StoryletEffectBatch(new StoryletEffect[]
                    {
                        new RemoveEntityTagEffect(PatientRoleId, WoundedTag),
                        new AddEntityAttributeEffect(PatientRoleId, HealthAttributeId, 25f),
                        new AddRelationTagEffect(PatientRoleId, HealerRoleId, GratitudeRelationTag)
                    }),
                    StoryletRepeatabilityPolicy.WithCooldown(1),
                    new StoryletSaliencePolicy(10f, 5f, 3f, 0.5f)),

                new(
                    SearchForMissingChildStoryletId,
                    "storylet.search_for_missing_child",
                    2.4f,
                    new List<Role>
                    {
                        new(SearcherRoleId, "role.searcher", QueryWithAllOf(WarriorTag)),
                        new(GuardianRoleId, "role.guardian", QueryWithAllOf(ChildTag | MissingTag))
                    },
                    new IStoryletPrecondition[]
                    {
                        new EntityTagPrecondition(QueryWithAllOf(MissingTag), mustExist: true, "A missing child must exist.")
                    },
                    new StoryletEffectBatch(new StoryletEffect[]
                    {
                        new RemoveEntityTagEffect(GuardianRoleId, MissingTag),
                        new AddEntityTagEffect(GuardianRoleId, RescuedTag),
                        new AddRelationTagEffect(GuardianRoleId, SearcherRoleId, TrustRelationTag)
                    }),
                    StoryletRepeatabilityPolicy.OncePerRun(),
                    new StoryletSaliencePolicy(5f, 1f, 3f, 0.5f)),

                new(
                    BanishTheRaiderStoryletId,
                    "storylet.banish_the_raider",
                    2.8f,
                    new List<Role>
                    {
                        new(JudgeRoleId, "role.judge", QueryWithAnyOf(NobleTag | WarriorTag)),
                        new(OutlawRoleId, "role.outlaw", QueryWithAllOf(OutlawTag))
                    },
                    new IStoryletPrecondition[]
                    {
                        new EntityTagPrecondition(QueryWithAllOf(OutlawTag), mustExist: true, "An outlaw raider must exist.")
                    },
                    new StoryletEffectBatch(new StoryletEffect[]
                    {
                        new DespawnEntityEffect(OutlawRoleId)
                    }),
                    StoryletRepeatabilityPolicy.OncePerRun(),
                    new StoryletSaliencePolicy(1f, 0f, 3f, 0.5f))
            };
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

        private static TagQuery QueryWithAllOf(
            TagSet allOf,
            TagSet anyOf = default,
            TagSet noneOf = default)
        {
            return new TagQuery(allOf, anyOf, noneOf);
        }

        private static TagQuery QueryWithAnyOf(TagSet anyOf)
        {
            return new TagQuery(TagSet.Empty, anyOf, TagSet.Empty);
        }
    }
}
