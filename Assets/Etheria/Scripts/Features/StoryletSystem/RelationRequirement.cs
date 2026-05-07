using System;

namespace Etheria.Features.StoryletSystem
{
    public readonly struct RelationRequirement
    {
        public RelationRequirement(
            string targetRoleId,
            TagQuery relationQuery,
            RelationDirection direction)
        {
            if (string.IsNullOrWhiteSpace(targetRoleId))
            {
                throw new ArgumentException("Target role id must be provided.", nameof(targetRoleId));
            }

            TargetRoleId = targetRoleId;
            RelationQuery = relationQuery;
            Direction = direction;
        }

        public string TargetRoleId { get; }
        public TagQuery RelationQuery { get; }
        public RelationDirection Direction { get; }
    }
}
