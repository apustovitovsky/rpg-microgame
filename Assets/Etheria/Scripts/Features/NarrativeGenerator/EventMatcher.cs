namespace Etheria.Features.NarrativeGeneration
{
    public static class EventMatcher
    {
        public static bool TryMatch(
            in WorldState worldState,
            in CompiledEventDefinition eventDefinition,
            out RelationContext participantContext,
            out EntityId matchedInitiator)
        {
            if (!TryMatchInitiator(worldState, eventDefinition.InitiatorQuery, out matchedInitiator))
            {
                participantContext = default;
                return false;
            }

            return TryMatchRelationChain(worldState, eventDefinition, matchedInitiator, out participantContext);
        }

        public static bool TryMatchInitiator(
            in WorldState worldState,
            TagQuery initiatorQuery,
            out EntityId matchedInitiator)
        {
            for (var entityIndex = 0; entityIndex < worldState.EntityCount; entityIndex++)
            {
                var entity = worldState.GetEntity(entityIndex);
                if (!initiatorQuery.Matches(entity.EffectiveTags))
                {
                    continue;
                }

                matchedInitiator = entity.Id;
                return true;
            }

            matchedInitiator = EntityId.Invalid;
            return false;
        }

        public static bool TryMatchRelationChain(
            in WorldState worldState,
            in CompiledEventDefinition eventDefinition,
            EntityId initiator,
            out RelationContext participantContext)
        {
            return RelationResolver.TryResolve(
                initiator,
                worldState,
                eventDefinition.RelationNodes,
                out participantContext);
        }
    }
}
