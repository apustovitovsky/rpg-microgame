using System;

namespace Etheria.Features.StoryletSystem
{
    public readonly struct EntityRelation
    {
        public EntityRelation(EntityId fromEntityId, EntityId toEntityId, TagSet tags)
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
