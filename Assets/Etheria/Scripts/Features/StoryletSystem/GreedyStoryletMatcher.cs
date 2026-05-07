using System;
using System.Collections.Generic;
using System.Linq;

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
            return Match(entities, storylets, relationIndex: null);
        }

        public StoryletMatchResult Match(
            IReadOnlyList<Entity> entities,
            IReadOnlyList<Storylet> storylets,
            IReadOnlyList<EntityRelation> relations)
        {
            var relationIndex = relations == null
                ? null
                : new RelationIndex(relations);

            return Match(entities, storylets, relationIndex);
        }

        public StoryletMatchResult Match(
            IReadOnlyList<Entity> entities,
            IReadOnlyList<Storylet> storylets,
            RelationIndex relationIndex)
        {
            var context = new StoryletMatchingContext(storylets, entities, relationIndex);

            if (!context.ValidateEntities(entities, out _))
            {
                return new StoryletMatchResult();
            }

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

            foreach (var storylet in remainingStorylets)
            {
                result.AddUnmatched(storylet, DescribeUnmatchedStorylet(context, storylet));
            }

            return result;
        }

        private bool CanActivateStorylet(
            StoryletMatchingContext context,
            Storylet storylet)
        {
            return context.ValidateStorylet(storylet, out _);
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
            var relationScore = context.EvaluateAssignmentRelationScore(assignment);

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
                + fitScore * 0.1f
                + relationScore * 25f;
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

        private string DescribeUnmatchedStorylet(
            StoryletMatchingContext context,
            Storylet storylet)
        {
            if (!context.ValidateStorylet(storylet, out var error))
            {
                return error;
            }

            var roleKeys = string.Join(", ", storylet.Roles.Select(role => role.Key));
            return $"No feasible relation-aware assignment remained for roles [{roleKeys}].";
        }
    }
}
