using System;
namespace Etheria.Features.StoryletSystem
{
    public sealed class Role
    {
        public Role(
            string id,
            TagQuery query,
            AttributeRequirement[] attributeRequirements = null,
            AttributePreference[] attributePreferences = null)
        {
            Id = id;
            Query = query;
            AttributeRequirements = attributeRequirements ?? EmptyAttributeRequirements;
            AttributePreferences = attributePreferences ?? EmptyAttributePreferences;
        }

        public string Id { get; }
        public TagQuery Query { get; }
        public AttributeRequirement[] AttributeRequirements { get; }
        public AttributePreference[] AttributePreferences { get; }

        private static AttributeRequirement[] EmptyAttributeRequirements { get; } =
            Array.Empty<AttributeRequirement>();

        private static AttributePreference[] EmptyAttributePreferences { get; } =
            Array.Empty<AttributePreference>();
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
