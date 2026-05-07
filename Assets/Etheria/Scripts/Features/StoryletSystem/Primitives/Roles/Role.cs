using System;
namespace Etheria.Features.StoryletSystem
{
    public sealed class Role
    {
        public Role(
            RoleId id,
            string key,
            TagQuery query,
            AttributeRequirement[] attributeRequirements = null,
            AttributePreference[] attributePreferences = null,
            RelationRequirement[] relationRequirements = null)
        {
            Id = id;
            Key = key;
            Query = query;
            AttributeRequirements = attributeRequirements ?? EmptyAttributeRequirements;
            AttributePreferences = attributePreferences ?? EmptyAttributePreferences;
            RelationRequirements = relationRequirements ?? EmptyRelationRequirements;
        }

        public RoleId Id { get; }
        public string Key { get; }
        public TagQuery Query { get; }
        public AttributeRequirement[] AttributeRequirements { get; }
        public AttributePreference[] AttributePreferences { get; }
        public RelationRequirement[] RelationRequirements { get; }

        private static AttributeRequirement[] EmptyAttributeRequirements { get; } =
            Array.Empty<AttributeRequirement>();

        private static AttributePreference[] EmptyAttributePreferences { get; } =
            Array.Empty<AttributePreference>();

        private static RelationRequirement[] EmptyRelationRequirements { get; } =
            Array.Empty<RelationRequirement>();

        public override string ToString()
        {
            return Key;
        }
    }
}
