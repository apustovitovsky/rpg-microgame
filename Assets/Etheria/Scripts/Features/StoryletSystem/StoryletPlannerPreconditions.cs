using System;
using System.Collections.Generic;

namespace Etheria.Features.StoryletSystem
{
    public interface IStoryletPrecondition
    {
        bool IsSatisfied(
            StoryletWorldState worldState,
            StoryletPlannerMemory memory,
            out StoryletRejectionReason rejectionReason);
    }

    public sealed class EntityTagPrecondition : IStoryletPrecondition
    {
        private readonly TagQuery _query;
        private readonly bool _mustExist;

        public EntityTagPrecondition(TagQuery query, bool mustExist, string description)
        {
            _query = query;
            _mustExist = mustExist;
            Description = description;
        }

        public string Description { get; }

        public bool IsSatisfied(
            StoryletWorldState worldState,
            StoryletPlannerMemory memory,
            out StoryletRejectionReason rejectionReason)
        {
            var exists = worldState.HasAnyEntity(_query);
            var isSatisfied = _mustExist ? exists : !exists;

            if (!isSatisfied)
            {
                rejectionReason = new StoryletRejectionReason(
                    "missing_entity_tag",
                    Description);
                return false;
            }

            rejectionReason = null;
            return true;
        }
    }

    public sealed class WorldTagPrecondition : IStoryletPrecondition
    {
        private readonly TagQuery _query;

        public WorldTagPrecondition(TagQuery query, string description)
        {
            _query = query;
            Description = description;
        }

        public string Description { get; }

        public bool IsSatisfied(
            StoryletWorldState worldState,
            StoryletPlannerMemory memory,
            out StoryletRejectionReason rejectionReason)
        {
            if (!_query.Matches(worldState.WorldTags))
            {
                rejectionReason = new StoryletRejectionReason("missing_world_tag", Description);
                return false;
            }

            rejectionReason = null;
            return true;
        }
    }

    public sealed class WorldAttributePrecondition : IStoryletPrecondition
    {
        private readonly AttributeRequirement _requirement;
        private readonly string _description;

        public WorldAttributePrecondition(AttributeRequirement requirement, string description)
        {
            _requirement = requirement;
            _description = description;
        }

        public bool IsSatisfied(
            StoryletWorldState worldState,
            StoryletPlannerMemory memory,
            out StoryletRejectionReason rejectionReason)
        {
            if (!worldState.WorldAttributes.Matches(_requirement))
            {
                rejectionReason = new StoryletRejectionReason(
                    "missing_attribute_threshold",
                    _description);
                return false;
            }

            rejectionReason = null;
            return true;
        }
    }

    public sealed class EntityExistencePrecondition : IStoryletPrecondition
    {
        private readonly EntityId _entityId;
        private readonly bool _mustExist;
        private readonly string _description;

        public EntityExistencePrecondition(EntityId entityId, bool mustExist, string description)
        {
            _entityId = entityId;
            _mustExist = mustExist;
            _description = description;
        }

        public bool IsSatisfied(
            StoryletWorldState worldState,
            StoryletPlannerMemory memory,
            out StoryletRejectionReason rejectionReason)
        {
            var exists = worldState.ContainsEntity(_entityId);

            if (exists != _mustExist)
            {
                rejectionReason = new StoryletRejectionReason("entity_existence", _description);
                return false;
            }

            rejectionReason = null;
            return true;
        }
    }

    public sealed class RelationPrecondition : IStoryletPrecondition
    {
        private readonly EntityId _fromEntityId;
        private readonly EntityId _toEntityId;
        private readonly TagQuery _query;
        private readonly bool _mustExist;
        private readonly string _description;

        public RelationPrecondition(
            EntityId fromEntityId,
            EntityId toEntityId,
            TagQuery query,
            bool mustExist,
            string description)
        {
            _fromEntityId = fromEntityId;
            _toEntityId = toEntityId;
            _query = query;
            _mustExist = mustExist;
            _description = description;
        }

        public bool IsSatisfied(
            StoryletWorldState worldState,
            StoryletPlannerMemory memory,
            out StoryletRejectionReason rejectionReason)
        {
            var relationIndex = worldState.CreateRelationIndex();
            var matches = _query.Matches(relationIndex.GetRelationTags(_fromEntityId, _toEntityId));

            if (matches != _mustExist)
            {
                rejectionReason = new StoryletRejectionReason("missing_relation", _description);
                return false;
            }

            rejectionReason = null;
            return true;
        }
    }
}
