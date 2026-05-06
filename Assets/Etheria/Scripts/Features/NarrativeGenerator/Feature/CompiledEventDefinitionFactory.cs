using Unity.Collections;

namespace Etheria.Features.NarrativeGeneration
{
    public sealed class CompiledEventDefinitionFactory : ICompiledEventDefinitionFactory
    {
        public CompiledEventDefinition CreateBanditsRaidFarm(Allocator allocator)
        {
            var relationNodes = new NativeArray<RelationStep>(2, allocator);

            relationNodes[0] = new RelationStep
            {
                SourceSlot = 0,
                Direction = RelationDirection.Outgoing,
                RelationTagQuery = new TagQuery
                {
                    AllOf = PrototypeTags.Threatens,
                    AnyOf = TagSet.None,
                    NoneOf = TagSet.None
                },
                TargetEntityTagQuery = new TagQuery
                {
                    AllOf = PrototypeTags.Farm,
                    AnyOf = TagSet.None,
                    NoneOf = PrototypeTags.Destroyed
                },
                BindTargetSlot = 1
            };

            relationNodes[1] = new RelationStep
            {
                SourceSlot = 1,
                Direction = RelationDirection.Outgoing,
                RelationTagQuery = new TagQuery
                {
                    AllOf = PrototypeTags.BelongsTo,
                    AnyOf = TagSet.None,
                    NoneOf = TagSet.None
                },
                TargetEntityTagQuery = new TagQuery
                {
                    AllOf = PrototypeTags.Village,
                    AnyOf = TagSet.None,
                    NoneOf = PrototypeTags.Destroyed
                },
                BindTargetSlot = 2
            };

            return new CompiledEventDefinition
            {
                InitiatorQuery = new TagQuery
                {
                    AllOf = PrototypeTags.BanditHideout,
                    AnyOf = TagSet.None,
                    NoneOf = TagSet.None
                },
                RelationNodes = relationNodes
            };
        }
    }
}
