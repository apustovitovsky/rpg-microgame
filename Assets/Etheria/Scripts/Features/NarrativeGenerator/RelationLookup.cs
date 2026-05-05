using System;
using Unity.Collections;
using Unity.Jobs;

namespace Etheria.Features.NarrativeGeneration
{
    public struct RelationLookup : IDisposable
    {
        private NativeParallelMultiHashMap<int, int> _outgoingByEntity;
        private NativeParallelMultiHashMap<int, int> _incomingByEntity;

        public RelationLookup(
            NativeParallelMultiHashMap<int, int> outgoingByEntity,
            NativeParallelMultiHashMap<int, int> incomingByEntity)
        {

            _outgoingByEntity = outgoingByEntity;
            _incomingByEntity = incomingByEntity;
        }

        public readonly bool IsCreated =>
            _outgoingByEntity.IsCreated ||
            _incomingByEntity.IsCreated;

        public void Clear()
        {
            if (_outgoingByEntity.IsCreated)
            {
                _outgoingByEntity.Clear();
            }

            if (_incomingByEntity.IsCreated)
            {
                _incomingByEntity.Clear();
            }
        }

        public void AddRelation(RelationRecord relation, int relationId)
        {
            _outgoingByEntity.Add(relation.Source.Value, relationId);
            _incomingByEntity.Add(relation.Target.Value, relationId);
        }

        public readonly bool TryGetFirstOutgoingRelation(
            EntityId entityId,
            out int relationId,
            out NativeParallelMultiHashMapIterator<int> iterator)
        {
            return _outgoingByEntity.TryGetFirstValue(entityId.Value, out relationId, out iterator);
        }

        public readonly bool TryGetNextOutgoingRelation(
            out int relationId,
            ref NativeParallelMultiHashMapIterator<int> iterator)
        {
            return _outgoingByEntity.TryGetNextValue(out relationId, ref iterator);
        }

        public readonly bool TryGetFirstIncomingRelation(
            EntityId entityId,
            out int relationId,
            out NativeParallelMultiHashMapIterator<int> iterator)
        {
            return _incomingByEntity.TryGetFirstValue(entityId.Value, out relationId, out iterator);
        }

        public readonly bool HasOutgoingRelations(EntityId entityId)
        {
            return _outgoingByEntity.ContainsKey(entityId.Value);
        }

        public readonly bool HasIncomingRelations(EntityId entityId)
        {
            return _incomingByEntity.ContainsKey(entityId.Value);
        }

        public readonly bool TryGetNextIncomingRelation(
            out int relationId,
            ref NativeParallelMultiHashMapIterator<int> iterator)
        {
            return _incomingByEntity.TryGetNextValue(out relationId, ref iterator);
        }

        public void Dispose()
        {
            if (_outgoingByEntity.IsCreated)
            {
                _outgoingByEntity.Dispose();
            }

            if (_incomingByEntity.IsCreated)
            {
                _incomingByEntity.Dispose();
            }
        }

        public JobHandle Dispose(JobHandle dependency)
        {
            var handle = dependency;

            if (_outgoingByEntity.IsCreated)
            {
                handle = _outgoingByEntity.Dispose(handle);
            }

            if (_incomingByEntity.IsCreated)
            {
                handle = _incomingByEntity.Dispose(handle);
            }

            return handle;
        }
    }
}
