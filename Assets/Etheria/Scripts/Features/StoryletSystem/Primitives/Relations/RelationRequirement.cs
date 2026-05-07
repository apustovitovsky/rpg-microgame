using System;

namespace Etheria.Features.StoryletSystem
{
    public readonly struct RelationRequirement
    {
        public RelationRequirement(
            RoleId targetRoleId,
            TagQuery relationQuery,
            RelationDirection direction)
        {
            TargetRoleId = targetRoleId;
            RelationQuery = relationQuery;
            Direction = direction;
        }

        public RoleId TargetRoleId { get; }
        public TagQuery RelationQuery { get; }
        public RelationDirection Direction { get; }
    }
}
