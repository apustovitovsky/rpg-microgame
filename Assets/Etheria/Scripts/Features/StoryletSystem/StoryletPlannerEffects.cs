using System;

namespace Etheria.Features.StoryletSystem
{
    public abstract class StoryletEffect
    {
    }

    public abstract class EntityTargetedEffect : StoryletEffect
    {
        protected EntityTargetedEffect(RoleId roleId)
        {
            RoleId = roleId;
        }

        public RoleId RoleId { get; }
    }

    public sealed class AddEntityTagEffect : EntityTargetedEffect
    {
        public AddEntityTagEffect(RoleId roleId, TagSet tag) : base(roleId)
        {
            Tag = tag;
        }

        public TagSet Tag { get; }
    }

    public sealed class RemoveEntityTagEffect : EntityTargetedEffect
    {
        public RemoveEntityTagEffect(RoleId roleId, TagSet tag) : base(roleId)
        {
            Tag = tag;
        }

        public TagSet Tag { get; }
    }

    public sealed class SetEntityAttributeEffect : EntityTargetedEffect
    {
        public SetEntityAttributeEffect(RoleId roleId, AttributeId attributeId, float value) : base(roleId)
        {
            AttributeId = attributeId;
            Value = value;
        }

        public AttributeId AttributeId { get; }
        public float Value { get; }
    }

    public sealed class AddEntityAttributeEffect : EntityTargetedEffect
    {
        public AddEntityAttributeEffect(RoleId roleId, AttributeId attributeId, float delta) : base(roleId)
        {
            AttributeId = attributeId;
            Delta = delta;
        }

        public AttributeId AttributeId { get; }
        public float Delta { get; }
    }

    public sealed class RemoveEntityAttributeEffect : EntityTargetedEffect
    {
        public RemoveEntityAttributeEffect(RoleId roleId, AttributeId attributeId) : base(roleId)
        {
            AttributeId = attributeId;
        }

        public AttributeId AttributeId { get; }
    }

    public sealed class SetWorldAttributeEffect : StoryletEffect
    {
        public SetWorldAttributeEffect(AttributeId attributeId, float value)
        {
            AttributeId = attributeId;
            Value = value;
        }

        public AttributeId AttributeId { get; }
        public float Value { get; }
    }

    public sealed class AddWorldAttributeEffect : StoryletEffect
    {
        public AddWorldAttributeEffect(AttributeId attributeId, float delta)
        {
            AttributeId = attributeId;
            Delta = delta;
        }

        public AttributeId AttributeId { get; }
        public float Delta { get; }
    }

    public sealed class SpawnEntityEffect : StoryletEffect
    {
        public SpawnEntityEffect(Entity entity)
        {
            Entity = entity ?? throw new ArgumentNullException(nameof(entity));
        }

        public Entity Entity { get; }
    }

    public sealed class DespawnEntityEffect : EntityTargetedEffect
    {
        public DespawnEntityEffect(RoleId roleId) : base(roleId)
        {
        }
    }

    public sealed class CreateRelationEffect : StoryletEffect
    {
        public CreateRelationEffect(RoleId fromRoleId, RoleId toRoleId, TagSet tags)
        {
            FromRoleId = fromRoleId;
            ToRoleId = toRoleId;
            Tags = tags;
        }

        public RoleId FromRoleId { get; }
        public RoleId ToRoleId { get; }
        public TagSet Tags { get; }
    }

    public sealed class RemoveRelationEffect : StoryletEffect
    {
        public RemoveRelationEffect(RoleId fromRoleId, RoleId toRoleId)
        {
            FromRoleId = fromRoleId;
            ToRoleId = toRoleId;
        }

        public RoleId FromRoleId { get; }
        public RoleId ToRoleId { get; }
    }

    public sealed class AddRelationTagEffect : StoryletEffect
    {
        public AddRelationTagEffect(RoleId fromRoleId, RoleId toRoleId, TagSet tag)
        {
            FromRoleId = fromRoleId;
            ToRoleId = toRoleId;
            Tag = tag;
        }

        public RoleId FromRoleId { get; }
        public RoleId ToRoleId { get; }
        public TagSet Tag { get; }
    }

    public sealed class RemoveRelationTagEffect : StoryletEffect
    {
        public RemoveRelationTagEffect(RoleId fromRoleId, RoleId toRoleId, TagSet tag)
        {
            FromRoleId = fromRoleId;
            ToRoleId = toRoleId;
            Tag = tag;
        }

        public RoleId FromRoleId { get; }
        public RoleId ToRoleId { get; }
        public TagSet Tag { get; }
    }
}
