using System;
using System.Collections.Generic;

namespace Etheria.Features.StoryletSystem
{
    public sealed class GreedyStoryletMatcher
    {
        private readonly IStoryletAssignmentBuilder _assignmentBuilder;
        private readonly IEntityRoleFitEvaluator _entityRoleFitEvaluator;

        public GreedyStoryletMatcher(
            IStoryletAssignmentBuilder assignmentBuilder,
            IEntityRoleFitEvaluator entityRoleFitEvaluator)
        {
            _assignmentBuilder = assignmentBuilder;
            _entityRoleFitEvaluator = entityRoleFitEvaluator;
        }

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

                    if (!_assignmentBuilder.TryBuildAssignment(
                        context,
                        storylet,
                        freeEntities,
                        out var assignment))
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
                fitScore += _entityRoleFitEvaluator.Evaluate(roleAssignment.Entity, roleAssignment.Role);
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

                if (_assignmentBuilder.TryBuildAssignment(
                    context,
                    otherStorylet,
                    simulatedFreeEntities,
                    out _))
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
    }
}
