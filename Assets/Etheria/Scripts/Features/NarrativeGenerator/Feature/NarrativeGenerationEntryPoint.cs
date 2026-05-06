using Unity.Collections;
using VContainer.Unity;

namespace Etheria.Features.NarrativeGeneration
{
    public sealed class NarrativeGenerationEntryPoint : IStartable
    {
        private readonly INarrativeGenerationService _generationService;

        public NarrativeGenerationEntryPoint(INarrativeGenerationService generationService)
        {
            _generationService = generationService;
        }

        public void Start()
        {
            var worldState = CreatePrototypeWorldState();
            var result = _generationService.RunSimulation(worldState, dayCount: 10, allocator: Allocator.TempJob);

            result.Dispose();
        }

        private static WorldState CreatePrototypeWorldState()
        {
            var entities = new NativeArray<EntityRecord>(3, Allocator.TempJob);
            entities[0] = new EntityRecord
            {
                Id = new EntityId(0),
                EffectiveTags = PrototypeTags.BanditHideout
            };
            entities[1] = new EntityRecord
            {
                Id = new EntityId(1),
                EffectiveTags = PrototypeTags.Farm
            };
            entities[2] = new EntityRecord
            {
                Id = new EntityId(2),
                EffectiveTags = PrototypeTags.Village
            };

            var relations = new NativeArray<RelationRecord>(2, Allocator.TempJob);
            relations[0] = new RelationRecord
            {
                Source = new EntityId(0),
                Target = new EntityId(1),
                RelationTags = PrototypeTags.Threatens
            };
            relations[1] = new RelationRecord
            {
                Source = new EntityId(1),
                Target = new EntityId(2),
                RelationTags = PrototypeTags.BelongsTo
            };

            var relationLookup = new RelationLookup(
                new NativeParallelMultiHashMap<int, int>(relations.Length, Allocator.TempJob),
                new NativeParallelMultiHashMap<int, int>(relations.Length, Allocator.TempJob));

            return new WorldState(entities, relations, relationLookup);
        }
    }
}
