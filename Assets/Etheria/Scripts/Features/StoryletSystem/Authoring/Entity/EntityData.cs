namespace Etheria.Features.StoryletSystem.Authoring
{
    public readonly struct EntityData
    {
        public EntityData(
            EntityId id,
            TagSet tags)
        {
            Id = id;
            Tags = tags;
        }

        public EntityId Id { get; }
        public TagSet Tags { get; }

        public bool CanFill(RoleData role)
        {
            return role.Matches(Tags);
        }
    }
}
