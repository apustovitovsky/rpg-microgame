using System;
namespace Etheria.Features.StoryletSystem
{
    public sealed class Role
    {
        public Role(
            string id,
            TagQuery query,
            AttributeRequirement[] attributeRequirements = null)
        {
            Id = id;
            Query = query;
            AttributeRequirements = attributeRequirements ?? EmptyAttributeRequirements;
        }

        public string Id { get; }
        public TagQuery Query { get; }
        public AttributeRequirement[] AttributeRequirements { get; }

        private static AttributeRequirement[] EmptyAttributeRequirements { get; } =
            Array.Empty<AttributeRequirement>();
    }


    public sealed class RoleAssignment
    {
        public RoleAssignment(Role role, Entity entity)
        {
            Role = role ?? throw new ArgumentNullException(nameof(role));
            Entity = entity ?? throw new ArgumentNullException(nameof(entity));
        }

        public Role Role { get; }
        public Entity Entity { get; }
    }
}
