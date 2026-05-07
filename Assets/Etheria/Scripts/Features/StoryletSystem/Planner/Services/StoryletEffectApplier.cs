using System;
using System.Collections.Generic;
using System.Linq;

namespace Etheria.Features.StoryletSystem
{
    public sealed class StoryletEffectApplier : IStoryletEffectApplier
    {
        public StoryletWorldState Apply(
            StoryletInstantiationCandidate candidate,
            StoryletWorldState worldState)
        {
            var entityMap = new Dictionary<EntityId, Entity>();

            foreach (var entity in worldState.Entities)
            {
                entityMap[entity.Id] = entity;
            }

            var relationMap = new Dictionary<(EntityId From, EntityId To), TagSet>();

            foreach (var relation in worldState.Relations)
            {
                relationMap[(relation.FromEntityId, relation.ToEntityId)] = relation.Tags;
            }

            var worldAttributes = EnsureWorldAttributeCapacity(
                worldState.WorldAttributes.ToArray(),
                candidate.EffectPreview.Effects);

            foreach (var effect in candidate.EffectPreview.Effects)
            {
                ApplyEffect(effect, candidate.Assignment, entityMap, relationMap, worldAttributes);
            }

            return new StoryletWorldState(
                worldState.SnapshotId + 1,
                entityMap.Values.OrderBy(entity => entity.Id.Value).ToList(),
                relationMap.Select(pair => new EntityRelation(pair.Key.From, pair.Key.To, pair.Value)).ToList(),
                new AttributeSet(worldAttributes),
                worldState.WorldTags);
        }

        private static void ApplyEffect(
            StoryletEffect effect,
            IReadOnlyList<RoleAssignment> assignment,
            Dictionary<EntityId, Entity> entityMap,
            Dictionary<(EntityId From, EntityId To), TagSet> relationMap,
            float[] worldAttributes)
        {
            switch (effect)
            {
                case AddEntityTagEffect addEntityTag:
                    UpdateEntity(assignment, entityMap, addEntityTag.RoleId, entity => new Entity(
                        entity.Id,
                        entity.Key,
                        entity.Tags | addEntityTag.Tag,
                        entity.Attributes));
                    break;
                case RemoveEntityTagEffect removeEntityTag:
                    UpdateEntity(assignment, entityMap, removeEntityTag.RoleId, entity => new Entity(
                        entity.Id,
                        entity.Key,
                        entity.Tags.Without(removeEntityTag.Tag),
                        entity.Attributes));
                    break;
                case SetEntityAttributeEffect setEntityAttribute:
                    UpdateEntityAttribute(assignment, entityMap, setEntityAttribute.RoleId, setEntityAttribute.AttributeId, setEntityAttribute.Value, replace: true);
                    break;
                case AddEntityAttributeEffect addEntityAttribute:
                    UpdateEntityAttributeDelta(assignment, entityMap, addEntityAttribute.RoleId, addEntityAttribute.AttributeId, addEntityAttribute.Delta);
                    break;
                case RemoveEntityAttributeEffect removeEntityAttribute:
                    RemoveEntityAttribute(assignment, entityMap, removeEntityAttribute.RoleId, removeEntityAttribute.AttributeId);
                    break;
                case SetWorldAttributeEffect setWorldAttribute:
                    SetAttributeValue(worldAttributes, setWorldAttribute.AttributeId, setWorldAttribute.Value, replace: true);
                    break;
                case AddWorldAttributeEffect addWorldAttribute:
                    SetAttributeValue(
                        worldAttributes,
                        addWorldAttribute.AttributeId,
                        GetAttributeValue(worldAttributes, addWorldAttribute.AttributeId) + addWorldAttribute.Delta,
                        replace: true);
                    break;
                case SpawnEntityEffect spawnEntity:
                    entityMap[spawnEntity.Entity.Id] = spawnEntity.Entity;
                    break;
                case DespawnEntityEffect despawnEntity:
                    if (TryGetEntityId(assignment, despawnEntity.RoleId, out var entityId))
                    {
                        entityMap.Remove(entityId);

                        foreach (var relationKey in relationMap.Keys.ToList())
                        {
                            if (relationKey.From == entityId || relationKey.To == entityId)
                            {
                                relationMap.Remove(relationKey);
                            }
                        }
                    }
                    break;
                case CreateRelationEffect createRelation:
                    if (TryResolveRelationEndpoints(assignment, createRelation.FromRoleId, createRelation.ToRoleId, out var createKey))
                    {
                        relationMap[createKey] = createRelation.Tags;
                    }
                    break;
                case RemoveRelationEffect removeRelation:
                    if (TryResolveRelationEndpoints(assignment, removeRelation.FromRoleId, removeRelation.ToRoleId, out var removeKey))
                    {
                        relationMap.Remove(removeKey);
                    }
                    break;
                case AddRelationTagEffect addRelationTag:
                    if (TryResolveRelationEndpoints(assignment, addRelationTag.FromRoleId, addRelationTag.ToRoleId, out var addKey))
                    {
                        relationMap[addKey] = relationMap.TryGetValue(addKey, out var currentTags)
                            ? currentTags | addRelationTag.Tag
                            : addRelationTag.Tag;
                    }
                    break;
                case RemoveRelationTagEffect removeRelationTag:
                    if (TryResolveRelationEndpoints(assignment, removeRelationTag.FromRoleId, removeRelationTag.ToRoleId, out var removeTagKey)
                        && relationMap.TryGetValue(removeTagKey, out var existingTags))
                    {
                        relationMap[removeTagKey] = existingTags.Without(removeRelationTag.Tag);
                    }
                    break;
            }
        }

        private static void UpdateEntity(
            IReadOnlyList<RoleAssignment> assignment,
            Dictionary<EntityId, Entity> entityMap,
            RoleId roleId,
            Func<Entity, Entity> updater)
        {
            if (TryGetEntityId(assignment, roleId, out var entityId) && entityMap.TryGetValue(entityId, out var entity))
            {
                entityMap[entityId] = updater(entity);
            }
        }

        private static void UpdateEntityAttribute(
            IReadOnlyList<RoleAssignment> assignment,
            Dictionary<EntityId, Entity> entityMap,
            RoleId roleId,
            AttributeId attributeId,
            float value,
            bool replace)
        {
            UpdateEntity(assignment, entityMap, roleId, entity =>
            {
                var updated = entity.Attributes.ToArray();
                updated = EnsureCapacity(updated, attributeId);
                updated[attributeId.Value] = replace ? value : updated[attributeId.Value] + value;
                return new Entity(entity.Id, entity.Key, entity.Tags, new AttributeSet(updated));
            });
        }

        private static void UpdateEntityAttributeDelta(
            IReadOnlyList<RoleAssignment> assignment,
            Dictionary<EntityId, Entity> entityMap,
            RoleId roleId,
            AttributeId attributeId,
            float delta)
        {
            UpdateEntity(assignment, entityMap, roleId, entity =>
            {
                var updated = entity.Attributes.ToArray();
                updated = EnsureCapacity(updated, attributeId);
                updated[attributeId.Value] += delta;
                return new Entity(entity.Id, entity.Key, entity.Tags, new AttributeSet(updated));
            });
        }

        private static void RemoveEntityAttribute(
            IReadOnlyList<RoleAssignment> assignment,
            Dictionary<EntityId, Entity> entityMap,
            RoleId roleId,
            AttributeId attributeId)
        {
            UpdateEntity(assignment, entityMap, roleId, entity =>
            {
                var updated = entity.Attributes.ToArray();

                if (attributeId.Value >= 0 && attributeId.Value < updated.Length)
                {
                    updated[attributeId.Value] = 0f;
                }

                return new Entity(entity.Id, entity.Key, entity.Tags, new AttributeSet(updated));
            });
        }

        private static bool TryResolveRelationEndpoints(
            IReadOnlyList<RoleAssignment> assignment,
            RoleId fromRoleId,
            RoleId toRoleId,
            out (EntityId From, EntityId To) key)
        {
            if (TryGetEntityId(assignment, fromRoleId, out var fromEntityId)
                && TryGetEntityId(assignment, toRoleId, out var toEntityId))
            {
                key = (fromEntityId, toEntityId);
                return true;
            }

            key = default;
            return false;
        }

        private static bool TryGetEntityId(
            IReadOnlyList<RoleAssignment> assignment,
            RoleId roleId,
            out EntityId entityId)
        {
            foreach (var roleAssignment in assignment)
            {
                if (roleAssignment.Role.Id == roleId)
                {
                    entityId = roleAssignment.Entity.Id;
                    return true;
                }
            }

            entityId = default;
            return false;
        }

        private static float[] EnsureCapacity(float[] values, AttributeId attributeId)
        {
            if (attributeId.Value < values.Length)
            {
                return values;
            }

            var resized = new float[attributeId.Value + 1];
            Array.Copy(values, resized, values.Length);
            return resized;
        }

        private static void SetAttributeValue(
            float[] values,
            AttributeId attributeId,
            float value,
            bool replace)
        {
            if (attributeId.Value >= values.Length)
            {
                throw new InvalidOperationException("World attribute array capacity is insufficient.");
            }

            values[attributeId.Value] = replace ? value : values[attributeId.Value] + value;
        }

        private static float GetAttributeValue(float[] values, AttributeId attributeId)
        {
            return attributeId.Value >= 0 && attributeId.Value < values.Length
                ? values[attributeId.Value]
                : 0f;
        }

        private static float[] EnsureWorldAttributeCapacity(
            float[] worldAttributes,
            IReadOnlyList<StoryletEffect> effects)
        {
            var maxIndex = worldAttributes.Length - 1;

            foreach (var effect in effects)
            {
                switch (effect)
                {
                    case SetWorldAttributeEffect setWorldAttribute:
                        maxIndex = Math.Max(maxIndex, setWorldAttribute.AttributeId.Value);
                        break;
                    case AddWorldAttributeEffect addWorldAttribute:
                        maxIndex = Math.Max(maxIndex, addWorldAttribute.AttributeId.Value);
                        break;
                }
            }

            if (maxIndex < worldAttributes.Length)
            {
                return worldAttributes;
            }

            var resized = new float[maxIndex + 1];
            Array.Copy(worldAttributes, resized, worldAttributes.Length);
            return resized;
        }
    }
}
