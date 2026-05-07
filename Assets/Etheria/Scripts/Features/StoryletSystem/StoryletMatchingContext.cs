using System;
using System.Collections.Generic;
using System.Linq;

namespace Etheria.Features.StoryletSystem
{
    public sealed class StoryletMatchingContext
    {
        private readonly List<Role> _allRoles = new();
        private readonly Dictionary<Role, List<Entity>> _compatibleEntitiesByRole = new();
        private readonly Dictionary<Storylet, Dictionary<RoleId, Role>> _rolesByIdByStorylet = new();

        public StoryletMatchingContext(
            IReadOnlyList<Storylet> storylets,
            IReadOnlyList<Entity> entities,
            RelationIndex relationIndex = null)
        {
            if (storylets == null)
            {
                throw new ArgumentNullException(nameof(storylets));
            }

            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            RelationIndex = relationIndex ?? new RelationIndex();

            foreach (var storylet in storylets)
            {
                if (storylet == null)
                {
                    continue;
                }

                RegisterStoryletRoles(storylet);

                foreach (var role in storylet.Roles)
                {
                    if (role == null || _compatibleEntitiesByRole.ContainsKey(role))
                    {
                        continue;
                    }

                    _allRoles.Add(role);

                    var compatibleEntities = new List<Entity>();

                    foreach (var entity in entities)
                    {
                        if (entity != null && entity.CanFill(role))
                        {
                            compatibleEntities.Add(entity);
                        }
                    }

                    _compatibleEntitiesByRole.Add(role, compatibleEntities);
                }
            }
        }

        public IReadOnlyList<Role> AllRoles => _allRoles;
        public RelationIndex RelationIndex { get; }

        public float GetEntityVersatility(Entity entity)
        {
            var count = 0;

            foreach (var role in _allRoles)
            {
                if (entity.CanFill(role))
                {
                    count++;
                }
            }

            return count;
        }

        public bool ValidateStorylet(Storylet storylet, out string error)
        {
            error = null;

            if (storylet == null)
            {
                error = "Storylet is null.";
                return false;
            }

            if (!_rolesByIdByStorylet.TryGetValue(storylet, out var rolesById))
            {
                error = $"Storylet '{storylet.Key}' has no registered roles.";
                return false;
            }

            if (rolesById.Count != storylet.Roles.Count)
            {
                error = $"Storylet '{storylet.Key}' contains duplicate role ids.";
                return false;
            }

            var roleKeys = new HashSet<string>(StringComparer.Ordinal);

            foreach (var role in storylet.Roles)
            {
                if (role == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(role.Key))
                {
                    error = $"Storylet '{storylet.Key}' contains role with empty key.";
                    return false;
                }

                if (!roleKeys.Add(role.Key))
                {
                    error = $"Storylet '{storylet.Key}' contains duplicate role keys.";
                    return false;
                }
            }

            foreach (var role in storylet.Roles)
            {
                if (role == null)
                {
                    continue;
                }

                foreach (var requirement in role.RelationRequirements)
                {
                    if (!rolesById.ContainsKey(requirement.TargetRoleId))
                    {
                        error =
                            $"Storylet '{storylet.Key}' role '{role.Key}' references missing target role '{requirement.TargetRoleId}'.";
                        return false;
                    }
                }
            }

            return true;
        }

        public bool ValidateEntities(IReadOnlyList<Entity> entities, out string error)
        {
            error = null;

            if (entities == null)
            {
                error = "Entities collection is null.";
                return false;
            }

            var entityIds = new HashSet<EntityId>();
            var entityKeys = new HashSet<string>(StringComparer.Ordinal);

            foreach (var entity in entities)
            {
                if (entity == null)
                {
                    continue;
                }

                if (!entityIds.Add(entity.Id))
                {
                    error = $"Duplicate entity runtime id '{entity.Id}'.";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(entity.Key))
                {
                    error = $"Entity '{entity.Id}' has empty key.";
                    return false;
                }

                if (!entityKeys.Add(entity.Key))
                {
                    error = $"Duplicate entity key '{entity.Key}'.";
                    return false;
                }
            }

            return true;
        }

        public IReadOnlyList<Entity> GetCompatibleEntities(Role role)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            return _compatibleEntitiesByRole.TryGetValue(role, out var compatibleEntities)
                ? compatibleEntities
                : Array.Empty<Entity>();
        }

        public int CountCompatibleEntities(
            Role role,
            HashSet<Entity> freeEntities)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            if (freeEntities == null)
            {
                throw new ArgumentNullException(nameof(freeEntities));
            }

            var count = 0;
            var compatibleEntities = GetCompatibleEntities(role);

            foreach (var entity in compatibleEntities)
            {
                if (freeEntities.Contains(entity))
                {
                    count++;
                }
            }

            return count;
        }

        public bool TryGetRoleById(Storylet storylet, RoleId roleId, out Role role)
        {
            role = null;

            if (storylet == null)
            {
                throw new ArgumentNullException(nameof(storylet));
            }

            return _rolesByIdByStorylet.TryGetValue(storylet, out var rolesById)
                && rolesById.TryGetValue(roleId, out role);
        }

        public bool TryGetAssignedEntity(
            IReadOnlyList<RoleAssignment> assignments,
            RoleId roleId,
            out Entity entity)
        {
            entity = null;

            if (assignments == null)
            {
                throw new ArgumentNullException(nameof(assignments));
            }

            foreach (var assignment in assignments)
            {
                if (assignment.Role.Id != roleId)
                {
                    continue;
                }

                entity = assignment.Entity;
                return true;
            }

            return false;
        }

        public bool IsRelationRequirementSatisfied(
            Entity self,
            Entity target,
            RelationRequirement requirement)
        {
            if (self == null)
            {
                throw new ArgumentNullException(nameof(self));
            }

            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            return requirement.Direction switch
            {
                RelationDirection.FromSelfToTarget =>
                    requirement.RelationQuery.Matches(RelationIndex.GetRelationTags(self, target)),
                RelationDirection.FromTargetToSelf =>
                    requirement.RelationQuery.Matches(RelationIndex.GetRelationTags(target, self)),
                RelationDirection.AnyDirection =>
                    requirement.RelationQuery.Matches(RelationIndex.GetRelationTags(self, target))
                    || requirement.RelationQuery.Matches(RelationIndex.GetRelationTags(target, self)),
                RelationDirection.BothDirections =>
                    requirement.RelationQuery.Matches(RelationIndex.GetRelationTags(self, target))
                    && requirement.RelationQuery.Matches(RelationIndex.GetRelationTags(target, self)),
                _ => false
            };
        }

        public bool HasPotentialTargetCandidate(
            Storylet storylet,
            IReadOnlyList<RoleAssignment> assignments,
            HashSet<Entity> freeEntities,
            HashSet<Entity> locallyUsedEntities,
            Role sourceRole,
            Entity sourceEntity,
            RelationRequirement requirement)
        {
            if (TryGetAssignedEntity(assignments, requirement.TargetRoleId, out var assignedTargetEntity))
            {
                return IsRelationRequirementSatisfied(sourceEntity, assignedTargetEntity, requirement);
            }

            if (!TryGetRoleById(storylet, requirement.TargetRoleId, out var targetRole))
            {
                return false;
            }

            foreach (var candidateEntity in GetCompatibleEntities(targetRole))
            {
                if (candidateEntity == sourceEntity)
                {
                    continue;
                }

                if (!freeEntities.Contains(candidateEntity))
                {
                    continue;
                }

                if (locallyUsedEntities.Contains(candidateEntity))
                {
                    continue;
                }

                if (IsRelationRequirementSatisfied(sourceEntity, candidateEntity, requirement))
                {
                    return true;
                }
            }

            return false;
        }

        public float EvaluateAssignmentRelationScore(IReadOnlyList<RoleAssignment> assignments)
        {
            if (assignments == null)
            {
                throw new ArgumentNullException(nameof(assignments));
            }

            var score = 0f;

            foreach (var assignment in assignments)
            {
                foreach (var requirement in assignment.Role.RelationRequirements)
                {
                    if (!TryGetAssignedEntity(assignments, requirement.TargetRoleId, out var targetEntity))
                    {
                        continue;
                    }

                    if (IsRelationRequirementSatisfied(assignment.Entity, targetEntity, requirement))
                    {
                        score += 1f;
                    }
                }
            }

            return score;
        }

        private void RegisterStoryletRoles(Storylet storylet)
        {
            var rolesById = new Dictionary<RoleId, Role>();

            foreach (var role in storylet.Roles.Where(role => role != null))
            {
                if (!rolesById.TryAdd(role.Id, role))
                {
                    continue;
                }
            }

            _rolesByIdByStorylet[storylet] = rolesById;
        }
    }
}
