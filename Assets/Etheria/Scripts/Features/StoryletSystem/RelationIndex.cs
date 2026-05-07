using System;
using System.Collections.Generic;

namespace Etheria.Features.StoryletSystem
{
    public sealed class RelationIndex
    {
        private readonly Dictionary<(EntityId From, EntityId To), TagSet> _relations = new();

        public RelationIndex(IReadOnlyList<EntityRelation> relations = null)
        {
            if (relations == null)
            {
                return;
            }

            foreach (var relation in relations)
            {
                Add(relation);
            }
        }

        public void Add(EntityRelation relation)
        {
            var key = (relation.FromEntityId, relation.ToEntityId);

            if (_relations.TryGetValue(key, out var tags))
            {
                _relations[key] = tags | relation.Tags;
                return;
            }

            _relations.Add(key, relation.Tags);
        }

        public TagSet GetRelationTags(EntityId fromEntityId, EntityId toEntityId)
        {
            return _relations.TryGetValue((fromEntityId, toEntityId), out var tags)
                ? tags
                : TagSet.Empty;
        }

        public TagSet GetRelationTags(Entity fromEntity, Entity toEntity)
        {
            if (fromEntity == null)
            {
                throw new ArgumentNullException(nameof(fromEntity));
            }

            if (toEntity == null)
            {
                throw new ArgumentNullException(nameof(toEntity));
            }

            return GetRelationTags(fromEntity.Id, toEntity.Id);
        }
    }
}
