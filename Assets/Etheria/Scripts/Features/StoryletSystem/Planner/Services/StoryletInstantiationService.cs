using System.Collections.Generic;

namespace Etheria.Features.StoryletSystem
{
    public sealed class StoryletInstantiationService : IStoryletInstantiationService
    {
        private readonly IStoryletAssignmentBuilder _assignmentBuilder;
        private readonly IEntityRoleFitEvaluator _entityRoleFitEvaluator;

        public StoryletInstantiationService(
            IStoryletAssignmentBuilder assignmentBuilder,
            IEntityRoleFitEvaluator entityRoleFitEvaluator)
        {
            _assignmentBuilder = assignmentBuilder;
            _entityRoleFitEvaluator = entityRoleFitEvaluator;
        }

        public StoryletInstantiationResult TryInstantiate(
            StoryletDefinition definition,
            StoryletWorldState worldState,
            StoryletPlannerMemory memory)
        {
            foreach (var precondition in definition.Preconditions)
            {
                if (!precondition.IsSatisfied(worldState, memory, out var rejectionReason))
                {
                    return new StoryletInstantiationResult(false, null, rejectionReason);
                }
            }

            var legacyStorylet = definition.ToStorylet();
            var context = new StoryletMatchingContext(
                new[] { legacyStorylet },
                worldState.Entities,
                worldState.CreateRelationIndex());
            var freeEntities = new HashSet<Entity>(worldState.Entities);

            if (!_assignmentBuilder.TryBuildAssignment(
                context,
                legacyStorylet,
                freeEntities,
                out var assignment))
            {
                return new StoryletInstantiationResult(
                    false,
                    null,
                    new StoryletRejectionReason(
                        "invalid_role_binding",
                        $"No feasible assignment remained for '{definition.Key}'."));
            }

            var instantiationQuality = EvaluateInstantiationQuality(context, definition, assignment);
            var candidate = new StoryletInstantiationCandidate(
                definition,
                legacyStorylet,
                assignment,
                definition.Effects,
                instantiationQuality,
                StoryletSalienceEvaluationPlaceholder.Value);

            return new StoryletInstantiationResult(true, candidate, null);
        }

        private float EvaluateInstantiationQuality(
            StoryletMatchingContext context,
            StoryletDefinition definition,
            IReadOnlyList<RoleAssignment> assignment)
        {
            float fit = 0f;

            foreach (var roleAssignment in assignment)
            {
                fit += _entityRoleFitEvaluator.Evaluate(roleAssignment.Entity, roleAssignment.Role);
            }

            var relationScore = context.EvaluateAssignmentRelationScore(assignment);
            return definition.Priority * 5f + fit + relationScore * 10f;
        }

        private static class StoryletSalienceEvaluationPlaceholder
        {
            public static readonly StoryletSalienceEvaluation Value = new(0f, 0f);
        }
    }
}
