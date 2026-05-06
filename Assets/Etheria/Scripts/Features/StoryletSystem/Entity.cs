namespace Etheria.Features.StoryletSystem
{
    public sealed class Entity
    {
        public Entity(
            string id,
            TagSet tags,
            AttributeSet attributes = default)
        {
            Id = id;
            Tags = tags;
            Attributes = attributes;
        }

        public string Id { get; }
        public TagSet Tags { get; }
        public AttributeSet Attributes { get; }

        public bool CanFill(Role role)
        {
            return role.Query.Matches(Tags)
                && Attributes.Matches(role.AttributeRequirements);
        }
    }

}
