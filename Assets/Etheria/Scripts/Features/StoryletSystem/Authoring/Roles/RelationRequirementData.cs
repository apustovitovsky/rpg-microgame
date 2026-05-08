using System;

namespace Etheria.Features.StoryletSystem.Authoring
{
    public readonly struct RelationRequirementData
    {
        public RelationRequirementData(
            ushort targetRoleIndex,
            TagRequirementData condition)
        {
            TargetRoleIndex = targetRoleIndex;
            Condition = condition;
        }

        public ushort TargetRoleIndex { get; }
        public TagRequirementData Condition { get; }
    }
}
