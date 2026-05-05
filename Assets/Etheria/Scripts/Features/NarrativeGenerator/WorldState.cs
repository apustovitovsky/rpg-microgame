using System;
using Unity.Collections;
using Unity.Jobs;

namespace Etheria.Features.NarrativeGeneration
{
    public struct WorldState : IDisposable
    {
        private NativeArray<EntityRecord> _entities;
        private NativeArray<RelationRecord> _relations;
        public RelationLookup RelationStore;

        public WorldState(
            NativeArray<EntityRecord> entities,
            NativeArray<RelationRecord> relations,
            RelationLookup relationStore)
        {
            _entities = entities;
            _relations = relations;
            RelationStore = relationStore;
        }

        public readonly bool IsCreated =>
            _entities.IsCreated ||
            _relations.IsCreated ||
            RelationStore.IsCreated;

        public readonly int RelationCount => _relations.IsCreated ? _relations.Length : 0;
        public readonly int EntityCount => _entities.IsCreated ? _entities.Length : 0;

        public readonly EntityRecord GetEntity(int entityId)
        {
            return _entities[entityId];
        }

        public void AddEntityTags(EntityId entityId, TagSet tags)
        {
            var entity = _entities[entityId.Value];
            entity.EffectiveTags |= tags;
            _entities[entityId.Value] = entity;
        }

        public readonly RelationRecord GetRelation(int relationId)
        {
            return _relations[relationId];
        }

        public void RebuildRelationLookup()
        {
            var relationLookup = RelationStore;
            relationLookup.Clear();

            for (var relationId = 0; relationId < RelationCount; relationId++)
            {
                var relation = GetRelation(relationId);
                relationLookup.AddRelation(relation, relationId);
            }

            RelationStore = relationLookup;
        }

        public void Dispose()
        {
            if (_entities.IsCreated)
            {
                _entities.Dispose();
            }

            if (_relations.IsCreated)
            {
                _relations.Dispose();
            }

            RelationStore.Dispose();
        }

        public JobHandle Dispose(JobHandle dependency)
        {
            var handle = dependency;

            if (_entities.IsCreated)
            {
                handle = _entities.Dispose(handle);
            }

            if (_relations.IsCreated)
            {
                handle = _relations.Dispose(handle);
            }

            handle = RelationStore.Dispose(handle);

            return handle;
        }
    }
}
