using System;
using System.Collections.Generic;
using System.Linq;

namespace Etheria.Features.StoryletSystem
{
    public sealed class GreedyStoryletAssignmentBuilder : IStoryletAssignmentBuilder
    {
        private readonly IEntityRoleFitEvaluator _entityRoleFitEvaluator;

        public GreedyStoryletAssignmentBuilder(IEntityRoleFitEvaluator entityRoleFitEvaluator)
        {
            _entityRoleFitEvaluator = entityRoleFitEvaluator;
        }

        public bool TryBuildAssignment(
            StoryletMatchingContext context,
            Storylet storylet,
            HashSet<Entity> freeEntities,
            out List<RoleAssignment> assignment)
        {
            if (!context.ValidateStorylet(storylet, out _))
            {
                assignment = null;
                return false;
            }

            assignment = new List<RoleAssignment>(storylet.Roles.Count);
            var locallyUsedEntities = new HashSet<Entity>();
            var unassignedRoles = new List<Role>(storylet.Roles);

            while (unassignedRoles.Count > 0)
            {
                Role bestRole = null;
                List<Entity> bestCandidates = null;

                foreach (var role in unassignedRoles)
                {
                    var candidates = GetValidCandidatesForPartialAssignment(
                        context,
                        storylet,
                        role,
                        assignment,
                        freeEntities,
                        locallyUsedEntities);

                    if (candidates.Count == 0)
                    {
                        assignment = null;
                        return false;
                    }

                    if (bestRole == null || candidates.Count < bestCandidates.Count)
                    {
                        bestRole = role;
                        bestCandidates = candidates;
                    }
                }

                if (bestRole == null || bestCandidates == null || bestCandidates.Count == 0)
                {
                    assignment = null;
                    return false;
                }

                var bestEntity = SelectBestEntity(
                    context,
                    storylet,
                    bestRole,
                    bestCandidates,
                    assignment,
                    freeEntities,
                    locallyUsedEntities);

                if (bestEntity == null)
                {
                    assignment = null;
                    return false;
                }

                locallyUsedEntities.Add(bestEntity);
                assignment.Add(new RoleAssignment(bestRole, bestEntity));
                unassignedRoles.Remove(bestRole);

                if (!IsPartialAssignmentStillPossible(
                    context,
                    storylet,
                    assignment,
                    freeEntities,
                    locallyUsedEntities))
                {
                    assignment = null;
                    return false;
                }
            }

            if (!IsFinalAssignmentValid(context, assignment))
            {
                assignment = null;
                return false;
            }

            return true;
        }

        private List<Entity> GetValidCandidatesForPartialAssignment(
            StoryletMatchingContext context,
            Storylet storylet,
            Role role,
            IReadOnlyList<RoleAssignment> partialAssignments,
            HashSet<Entity> freeEntities,
            HashSet<Entity> locallyUsedEntities)
        {
            var validCandidates = new List<Entity>();

            foreach (var entity in context.GetCompatibleEntities(role))
            {
                if (locallyUsedEntities.Contains(entity) || !freeEntities.Contains(entity))
                {
                    continue;
                }

                if (!IsEntityCompatibleWithAssignedRelations(context, role, entity, partialAssignments))
                {
                    continue;
                }

                var hypotheticalAssignments = new List<RoleAssignment>(partialAssignments)
                {
                    new(role, entity)
                };

                var hypotheticalLocallyUsed = new HashSet<Entity>(locallyUsedEntities)
                {
                    entity
                };

                if (!IsPartialAssignmentStillPossible(
                    context,
                    storylet,
                    hypotheticalAssignments,
                    freeEntities,
                    hypotheticalLocallyUsed))
                {
                    continue;
                }

                validCandidates.Add(entity);
            }

            return validCandidates;
        }

        private Entity SelectBestEntity(
            StoryletMatchingContext context,
            Storylet storylet,
            Role role,
            IReadOnlyList<Entity> candidates,
            IReadOnlyList<RoleAssignment> partialAssignments,
            HashSet<Entity> freeEntities,
            HashSet<Entity> locallyUsedEntities)
        {
            Entity bestEntity = null;
            var bestScore = float.NegativeInfinity;

            foreach (var candidate in candidates)
            {
                var score = EvaluateEntityScore(
                    context,
                    storylet,
                    candidate,
                    role,
                    partialAssignments,
                    freeEntities,
                    locallyUsedEntities);

                if (bestEntity == null || score > bestScore)
                {
                    bestEntity = candidate;
                    bestScore = score;
                }
            }

            return bestEntity;
        }

        private bool IsPartialAssignmentStillPossible(
            StoryletMatchingContext context,
            Storylet storylet,
            IReadOnlyList<RoleAssignment> assignments,
            HashSet<Entity> freeEntities,
            HashSet<Entity> locallyUsedEntities)
        {
            foreach (var assignment in assignments)
            {
                foreach (var requirement in assignment.Role.RelationRequirements)
                {
                    if (context.TryGetAssignedEntity(assignments, requirement.TargetRoleId, out var targetEntity))
                    {
                        if (!context.IsRelationRequirementSatisfied(assignment.Entity, targetEntity, requirement))
                        {
                            return false;
                        }

                        continue;
                    }

                    if (!context.HasPotentialTargetCandidate(
                        storylet,
                        assignments,
                        freeEntities,
                        locallyUsedEntities,
                        assignment.Role,
                        assignment.Entity,
                        requirement))
                    {
                        return false;
                    }
                }
            }

            foreach (var role in storylet.Roles)
            {
                if (assignments.Any(assignment => assignment.Role == role))
                {
                    continue;
                }

                if (!HasAnyFeasibleCandidateForRemainingRole(
                    context,
                    storylet,
                    role,
                    assignments,
                    freeEntities,
                    locallyUsedEntities))
                {
                    return false;
                }
            }

            return true;
        }

        private bool HasAnyFeasibleCandidateForRemainingRole(
            StoryletMatchingContext context,
            Storylet storylet,
            Role role,
            IReadOnlyList<RoleAssignment> assignments,
            HashSet<Entity> freeEntities,
            HashSet<Entity> locallyUsedEntities)
        {
            foreach (var entity in context.GetCompatibleEntities(role))
            {
                if (locallyUsedEntities.Contains(entity) || !freeEntities.Contains(entity))
                {
                    continue;
                }

                if (!IsEntityCompatibleWithAssignedRelations(context, role, entity, assignments))
                {
                    continue;
                }

                var hypotheticalAssignments = new List<RoleAssignment>(assignments)
                {
                    new(role, entity)
                };

                var hypotheticalLocallyUsed = new HashSet<Entity>(locallyUsedEntities)
                {
                    entity
                };

                var preservesFutureOptions = true;

                foreach (var requirement in role.RelationRequirements)
                {
                    if (context.TryGetAssignedEntity(hypotheticalAssignments, requirement.TargetRoleId, out var targetEntity))
                    {
                        if (!context.IsRelationRequirementSatisfied(entity, targetEntity, requirement))
                        {
                            preservesFutureOptions = false;
                            break;
                        }

                        continue;
                    }

                    if (!context.HasPotentialTargetCandidate(
                        storylet,
                        hypotheticalAssignments,
                        freeEntities,
                        hypotheticalLocallyUsed,
                        role,
                        entity,
                        requirement))
                    {
                        preservesFutureOptions = false;
                        break;
                    }
                }

                if (preservesFutureOptions)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsEntityCompatibleWithAssignedRelations(
            StoryletMatchingContext context,
            Role role,
            Entity entity,
            IReadOnlyList<RoleAssignment> partialAssignments)
        {
            foreach (var otherAssignment in partialAssignments)
            {
                foreach (var requirement in role.RelationRequirements)
                {
                    if (requirement.TargetRoleId != otherAssignment.Role.Id)
                    {
                        continue;
                    }

                    if (!context.IsRelationRequirementSatisfied(entity, otherAssignment.Entity, requirement))
                    {
                        return false;
                    }
                }

                foreach (var requirement in otherAssignment.Role.RelationRequirements)
                {
                    if (requirement.TargetRoleId != role.Id)
                    {
                        continue;
                    }

                    if (!context.IsRelationRequirementSatisfied(otherAssignment.Entity, entity, requirement))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool IsFinalAssignmentValid(
            StoryletMatchingContext context,
            IReadOnlyList<RoleAssignment> assignments)
        {
            foreach (var assignment in assignments)
            {
                foreach (var requirement in assignment.Role.RelationRequirements)
                {
                    if (!context.TryGetAssignedEntity(assignments, requirement.TargetRoleId, out var targetEntity))
                    {
                        return false;
                    }

                    if (!context.IsRelationRequirementSatisfied(assignment.Entity, targetEntity, requirement))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private float EvaluateEntityScore(
            StoryletMatchingContext context,
            Storylet storylet,
            Entity entity,
            Role role,
            IReadOnlyList<RoleAssignment> partialAssignments,
            HashSet<Entity> freeEntities,
            HashSet<Entity> locallyUsedEntities)
        {
            var fit = _entityRoleFitEvaluator.Evaluate(entity, role);
            var versatility = context.GetEntityVersatility(entity);
            var relationContribution = EvaluateRelationContribution(
                context,
                storylet,
                entity,
                role,
                partialAssignments,
                freeEntities,
                locallyUsedEntities);

            return fit * 4f
                + relationContribution
                - versatility * 1.5f;
        }

        private float EvaluateRelationContribution(
            StoryletMatchingContext context,
            Storylet storylet,
            Entity entity,
            Role role,
            IReadOnlyList<RoleAssignment> partialAssignments,
            HashSet<Entity> freeEntities,
            HashSet<Entity> locallyUsedEntities)
        {
            var score = 0f;

            foreach (var otherAssignment in partialAssignments)
            {
                foreach (var requirement in role.RelationRequirements)
                {
                    if (requirement.TargetRoleId != otherAssignment.Role.Id)
                    {
                        continue;
                    }

                    if (context.IsRelationRequirementSatisfied(entity, otherAssignment.Entity, requirement))
                    {
                        score += 12f;
                    }
                }

                foreach (var requirement in otherAssignment.Role.RelationRequirements)
                {
                    if (requirement.TargetRoleId != role.Id)
                    {
                        continue;
                    }

                    if (context.IsRelationRequirementSatisfied(otherAssignment.Entity, entity, requirement))
                    {
                        score += 12f;
                    }
                }
            }

            foreach (var requirement in role.RelationRequirements)
            {
                if (partialAssignments.Any(assignment => assignment.Role.Id == requirement.TargetRoleId))
                {
                    continue;
                }

                if (context.HasPotentialTargetCandidate(
                    storylet,
                    partialAssignments,
                    freeEntities,
                    locallyUsedEntities,
                    role,
                    entity,
                    requirement))
                {
                    score += 4f;
                }
            }

            return score;
        }
    }
}
