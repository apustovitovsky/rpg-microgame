using System;

namespace Etheria.Features.StoryletSystem
{
    public sealed class SpawnEntityEffect : StoryletEffect
    {
        public SpawnEntityEffect(Entity entity)
        {
            Entity = entity ?? throw new ArgumentNullException(nameof(entity));
        }

        public Entity Entity { get; }
    }
}
