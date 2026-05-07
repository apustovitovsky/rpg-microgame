using System;
using System.Collections.Generic;

namespace Etheria.Features.StoryletSystem
{
    public sealed class StoryletDefinition
    {
        public StoryletDefinition(
            StoryletId id,
            string key,
            float priority,
            IReadOnlyList<Role> roles,
            IReadOnlyList<IStoryletPrecondition> preconditions,
            StoryletEffectBatch effects,
            StoryletRepeatabilityPolicy repeatabilityPolicy,
            StoryletSaliencePolicy saliencePolicy)
        {
            Id = id;
            Key = key ?? throw new ArgumentNullException(nameof(key));
            Priority = priority;
            Roles = roles ?? throw new ArgumentNullException(nameof(roles));
            Preconditions = preconditions ?? Array.Empty<IStoryletPrecondition>();
            Effects = effects ?? throw new ArgumentNullException(nameof(effects));
            RepeatabilityPolicy = repeatabilityPolicy ?? StoryletRepeatabilityPolicy.OncePerRun();
            SaliencePolicy = saliencePolicy ?? StoryletSaliencePolicy.Default;
        }

        public StoryletId Id { get; }
        public string Key { get; }
        public float Priority { get; }
        public IReadOnlyList<Role> Roles { get; }
        public IReadOnlyList<IStoryletPrecondition> Preconditions { get; }
        public StoryletEffectBatch Effects { get; }
        public StoryletRepeatabilityPolicy RepeatabilityPolicy { get; }
        public StoryletSaliencePolicy SaliencePolicy { get; }

        public Storylet ToStorylet()
        {
            return new Storylet(Id, Key, Priority, Roles);
        }
    }
}
