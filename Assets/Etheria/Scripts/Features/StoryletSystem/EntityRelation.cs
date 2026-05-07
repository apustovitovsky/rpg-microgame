using System;

namespace Etheria.Features.StoryletSystem
{
    public readonly struct EntityRelation
    {
        public EntityRelation(string fromEntityId, string toEntityId, TagSet tags)
        {
            FromEntityId = fromEntityId ?? throw new ArgumentNullException(nameof(fromEntityId));
            ToEntityId = toEntityId ?? throw new ArgumentNullException(nameof(toEntityId));
            Tags = tags;
        }

        public string FromEntityId { get; }
        public string ToEntityId { get; }
        public TagSet Tags { get; }
    }
}
