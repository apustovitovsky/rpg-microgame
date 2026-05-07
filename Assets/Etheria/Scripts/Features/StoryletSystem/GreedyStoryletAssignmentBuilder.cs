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
            assignment = new List<RoleAssignment>();
            var locallyUsedEntities = new HashSet<Entity>();

            var orderedRoles = storylet.Roles
                .OrderBy(role => context.CountCompatibleEntities(role, freeEntities))
                .ToList();

            foreach (var role in orderedRoles)
            {
                Entity bestEntity = null;
                var bestCost = float.PositiveInfinity;

                foreach (var entity in context.GetCompatibleEntities(role))
                {
                    if (locallyUsedEntities.Contains(entity))
                    {
                        continue;
                    }

                    if (!freeEntities.Contains(entity))
                    {
                        continue;
                    }

                    var cost = EvaluateEntityCost(context, entity, role);

                    if (cost < bestCost)
                    {
                        bestCost = cost;
                        bestEntity = entity;
                    }
                }

                if (bestEntity == null)
                {
                    assignment = null;
                    return false;
                }

                locallyUsedEntities.Add(bestEntity);
                assignment.Add(new RoleAssignment(role, bestEntity));
            }

            return true;
        }

        private float EvaluateEntityCost(
            StoryletMatchingContext context,
            Entity entity,
            Role role)
        {
            var versatility = context.GetEntityVersatility(entity);
            var fit = _entityRoleFitEvaluator.Evaluate(entity, role);

            return versatility * 10f - fit;
        }
    }
}
