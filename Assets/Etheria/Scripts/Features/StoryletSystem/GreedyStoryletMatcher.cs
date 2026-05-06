using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

namespace Etheria.Features.StoryletSystem
{
    public sealed class GreedyStoryletMatcher
    {
        public StoryletMatchResult Match(
            IReadOnlyList<Entity> entities,
            IReadOnlyList<Storylet> storylets)
        {
            var context = new StoryletMatchingContext(storylets, entities);
            var freeEntities = new HashSet<Entity>(entities);
            var remainingStorylets = new HashSet<Storylet>(storylets);

            var result = new StoryletMatchResult();

            while (remainingStorylets.Count > 0)
            {
                StoryletCandidate bestCandidate = null;

                foreach (var storylet in remainingStorylets)
                {
                    if (!CanActivateStorylet(context, storylet))
                    {
                        continue;
                    }

                    if (!TryAssignStoryletGreedy(context, storylet, freeEntities, out var assignment))
                    {
                        continue;
                    }

                    var score = EvaluateStoryletCandidateWithLookahead(
                        context,
                        storylet,
                        assignment,
                        freeEntities,
                        remainingStorylets);

                    if (bestCandidate == null || score > bestCandidate.Score)
                    {
                        bestCandidate = new StoryletCandidate(
                            storylet,
                            assignment,
                            score);
                    }
                }

                if (bestCandidate == null)
                {
                    break;
                }

                result.Add(bestCandidate.Storylet, bestCandidate.Assignment);

                foreach (var roleAssignment in bestCandidate.Assignment)
                {
                    freeEntities.Remove(roleAssignment.Entity);
                }

                remainingStorylets.Remove(bestCandidate.Storylet);
            }

            return result;
        }

        private bool CanActivateStorylet(
            StoryletMatchingContext context,
            Storylet storylet)
        {
            return true;
        }

        private bool TryAssignStoryletGreedy(
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
                float bestCost = float.PositiveInfinity;

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

        private float EvaluateStoryletCandidate(
            StoryletMatchingContext context,
            Storylet storylet,
            IReadOnlyList<RoleAssignment> assignment,
            HashSet<Entity> freeEntities)
        {
            float roleScarcityCost = 0f;
            float entityOpportunityCost = 0f;
            float fitScore = 0f;

            foreach (var role in storylet.Roles)
            {
                var compatibleCount = context.CountCompatibleEntities(role, freeEntities);
                roleScarcityCost += 1f / Math.Max(1, compatibleCount);
            }

            foreach (var roleAssignment in assignment)
            {
                entityOpportunityCost += context.GetEntityVersatility(roleAssignment.Entity);
                fitScore += GetEntityRoleFit(roleAssignment.Entity, roleAssignment.Role);
            }

            return
                1000f
                - roleScarcityCost * 100f
                - entityOpportunityCost * 10f
                + storylet.Priority * 1f
                + fitScore * 0.1f;
        }

        private float EvaluateStoryletCandidateWithLookahead(
            StoryletMatchingContext context,
            Storylet storylet,
            IReadOnlyList<RoleAssignment> assignment,
            HashSet<Entity> freeEntities,
            IReadOnlyCollection<Storylet> remainingStorylets)
        {
            var simulatedFreeEntities = new HashSet<Entity>(freeEntities);

            foreach (var roleAssignment in assignment)
            {
                simulatedFreeEntities.Remove(roleAssignment.Entity);
            }

            var feasibleStoryletCountAfterAssignment = 0;

            foreach (var otherStorylet in remainingStorylets)
            {
                if (otherStorylet == storylet)
                {
                    continue;
                }

                if (TryAssignStoryletGreedy(context, otherStorylet, simulatedFreeEntities, out _))
                {
                    feasibleStoryletCountAfterAssignment++;
                }
            }

            var currentCandidateScore = EvaluateStoryletCandidate(
                context,
                storylet,
                assignment,
                freeEntities);

            return
                feasibleStoryletCountAfterAssignment * 100f
                + currentCandidateScore;
        }

        private float EvaluateEntityCost(
            StoryletMatchingContext context,
            Entity entity,
            Role role)
        {
            var versatility = context.GetEntityVersatility(entity);
            var fit = GetEntityRoleFit(entity, role);

            return
                versatility * 10f
                - fit * 1f;
        }

        private float GetEntityRoleFit(Entity entity, Role role)
        {
            if (role.AttributePreferences.Length == 0)
            {
                return 1f;
            }

            var weightedScoreSum = 0f;
            var absoluteWeightSum = 0f;

            foreach (var attributePreference in role.AttributePreferences)
            {
                var value = entity.Attributes.GetOrDefault(attributePreference.AttributeId);
                var factorScore = math.smoothstep(
                    attributePreference.Start,
                    attributePreference.End,
                    value);
                weightedScoreSum += factorScore * attributePreference.Weight;
                absoluteWeightSum += MathF.Abs(attributePreference.Weight);
            }

            if (absoluteWeightSum <= 0f)
            {
                return 1f;
            }

            var normalizedScore = weightedScoreSum / absoluteWeightSum;

            return 1f + normalizedScore;
        }
    }
}
