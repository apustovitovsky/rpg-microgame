namespace Etheria.Features.StoryletSystem
{
    public sealed class Entity
    {
        public Entity(string id, TagSet tags)
        {
            Id = id;
            Tags = tags;
        }

        public string Id { get; }
        public TagSet Tags { get; }

        public bool CanFill(Role role)
        {
            return role.Query.Matches(Tags);
        }
    }

}
