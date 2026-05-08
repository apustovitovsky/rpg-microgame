using System;

namespace Etheria.Features.StoryletSystem.Authoring
{
    public readonly struct StoryletData
    {
        public StoryletData(
            StoryletId id,
            TagMask storyletTags,
            TagRequirementData[] preconditions,
            RoleData[] roles,
            StoryletRepeatabilityPolicy repeatabilityPolicy,
            StoryletSaliencePolicy saliencePolicy)
        {
            Id = id;
            StoryletTags = storyletTags;
            Preconditions = preconditions ?? Array.Empty<TagRequirementData>();
            RoleSlots = roles ?? Array.Empty<RoleData>();
            RepeatabilityPolicy = repeatabilityPolicy;
            SaliencePolicy = saliencePolicy;
        }

        public readonly StoryletId Id { get; }
        public readonly TagMask StoryletTags { get; }
        public readonly TagRequirementData[] Preconditions { get; }
        public readonly RoleData[] RoleSlots { get; }
        public readonly StoryletRepeatabilityPolicy RepeatabilityPolicy { get; }
        public readonly StoryletSaliencePolicy SaliencePolicy { get; }
    }
}
