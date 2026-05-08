using System;
using System.Collections.Generic;

namespace Etheria.Features.StoryletSystem.Authoring
{
    public readonly struct RoleData
    {
        public RoleData(
            TagRequirementData[] tagRequirements,
            RelationRequirementData[] relationRequirements = null,
            ScoreBonusRule[] scoreBonuses = null)
        {
            TagRequirements = tagRequirements ?? Array.Empty<TagRequirementData>();
            RelationRequirements = relationRequirements ?? Array.Empty<RelationRequirementData>();
            ScoreBonuses = scoreBonuses ?? Array.Empty<ScoreBonusRule>();
        }

        public readonly TagRequirementData[] TagRequirements;
        public readonly RelationRequirementData[] RelationRequirements { get; }
        public readonly ScoreBonusRule[] ScoreBonuses { get; }

        public bool MatchesRelations(
            IReadOnlyList<EntityData> assignedEntities,
            IReadOnlyList<EntityRelationData> relations)
        {
            if (RelationRequirements == null || RelationRequirements.Length == 0)
            {
                return true;
            }

            if (assignedEntities == null)
            {
                return false;
            }

            for (var i = 0; i < RelationRequirements.Length; i++)
            {
                var requirement = RelationRequirements[i];

                if (requirement.TargetRoleIndex >= assignedEntities.Count)
                {
                    return false;
                }

                var targetEntity = assignedEntities[requirement.TargetRoleIndex];
                var relationTags = GetRelationTags(targetEntity.Id, relations);

                if (!requirement.Condition.Matches(relationTags))
                {
                    return false;
                }
            }

            return true;
        }

        public bool Matches(TagSet tags)
        {
            if (TagRequirements == null)
            {
                return true;
            }

            for (var i = 0; i < TagRequirements.Length; i++)
            {
                if (!TagRequirements[i].Matches(tags))
                {
                    return false;
                }
            }

            return true;
        }

        private static TagSet GetRelationTags(
            EntityId toEntityId,
            IReadOnlyList<EntityRelationData> relations)
        {
            if (relations == null)
            {
                return default;
            }

            for (var i = 0; i < relations.Count; i++)
            {
                var relation = relations[i];

                if (relation.ToEntityId.Equals(toEntityId))
                {
                    return relation.Tags;
                }
            }

            return default;
        }
    }
}
