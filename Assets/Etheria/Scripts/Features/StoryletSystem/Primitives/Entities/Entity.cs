namespace Etheria.Features.StoryletSystem
{
    public sealed class Entity
    {
        public Entity(
            EntityId id,
            string key,
            TagSet tags,
            AttributeSet attributes = default)
        {
            Id = id;
            Key = key;
            Tags = tags;
            Attributes = attributes;
        }

        public EntityId Id { get; }
        public string Key { get; }
        public TagSet Tags { get; }
        public AttributeSet Attributes { get; }

        public bool CanFill(Role role)
        {
            return role.Query.Matches(Tags)
                && Attributes.Matches(role.AttributeRequirements);
        }

        public override string ToString()
        {
            return Key;
        }
    }

}
