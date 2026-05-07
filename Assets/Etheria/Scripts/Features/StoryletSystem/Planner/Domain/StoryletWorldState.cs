using System;
using System.Collections.Generic;
using System.Linq;

namespace Etheria.Features.StoryletSystem
{
    public sealed class StoryletWorldState
    {
        private readonly Dictionary<EntityId, Entity> _entitiesById;

        public StoryletWorldState(
            int snapshotId,
            IReadOnlyList<Entity> entities,
            IReadOnlyList<EntityRelation> relations,
            AttributeSet worldAttributes,
            TagSet worldTags = default)
        {
            SnapshotId = snapshotId;
            Entities = entities ?? throw new ArgumentNullException(nameof(entities));
            Relations = relations ?? throw new ArgumentNullException(nameof(relations));
            WorldAttributes = worldAttributes;
            WorldTags = worldTags;
            _entitiesById = new Dictionary<EntityId, Entity>(entities.Count);

            foreach (var entity in entities)
            {
                if (entity != null)
                {
                    _entitiesById[entity.Id] = entity;
                }
            }
        }

        public int SnapshotId { get; }
        public IReadOnlyList<Entity> Entities { get; }
        public IReadOnlyList<EntityRelation> Relations { get; }
        public AttributeSet WorldAttributes { get; }
        public TagSet WorldTags { get; }

        public bool TryGetEntity(EntityId entityId, out Entity entity)
        {
            return _entitiesById.TryGetValue(entityId, out entity);
        }

        public bool ContainsEntity(EntityId entityId)
        {
            return _entitiesById.ContainsKey(entityId);
        }

        public bool HasAnyEntity(TagQuery query)
        {
            foreach (var entity in Entities)
            {
                if (entity != null && query.Matches(entity.Tags))
                {
                    return true;
                }
            }

            return false;
        }

        public int CountEntities(TagQuery query)
        {
            var count = 0;

            foreach (var entity in Entities)
            {
                if (entity != null && query.Matches(entity.Tags))
                {
                    count++;
                }
            }

            return count;
        }

        public RelationIndex CreateRelationIndex()
        {
            return new RelationIndex(Relations);
        }

        public string GetFingerprint()
        {
            var entityParts = new List<string>(Entities.Count);

            foreach (var entity in Entities.OrderBy(entity => entity.Id.Value))
            {
                entityParts.Add(
                    $"{entity.Id.Value}:{entity.Key}:{entity.Tags.GetHashCode()}:{GetAttributeFingerprint(entity.Attributes)}");
            }

            var relationParts = new List<string>(Relations.Count);

            foreach (var relation in Relations
                .OrderBy(relation => relation.FromEntityId.Value)
                .ThenBy(relation => relation.ToEntityId.Value))
            {
                relationParts.Add(
                    $"{relation.FromEntityId.Value}>{relation.ToEntityId.Value}:{relation.Tags.GetHashCode()}");
            }

            return string.Join("|", entityParts)
                + "#"
                + string.Join("|", relationParts)
                + "#"
                + WorldTags.GetHashCode()
                + "#"
                + GetAttributeFingerprint(WorldAttributes);
        }

        private static string GetAttributeFingerprint(AttributeSet attributes)
        {
            return string.Join(",", attributes.ToArray().Select(value => value.ToString("0.###")));
        }
    }
}
