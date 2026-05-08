namespace Etheria.Features.StoryletSystem.Authoring
{
    public readonly struct EntityRelationData
    {
        public EntityRelationData(
            EntityId fromEntityId,
            EntityId toEntityId,
            TagSet tags)
        {
            FromEntityId = fromEntityId;
            ToEntityId = toEntityId;
            Tags = tags;
        }

        public EntityId FromEntityId { get; }
        public EntityId ToEntityId { get; }
        public TagSet Tags { get; }
    }
}
