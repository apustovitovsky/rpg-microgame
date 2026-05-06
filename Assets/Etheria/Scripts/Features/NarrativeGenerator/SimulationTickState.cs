namespace Etheria.Features.NarrativeGeneration
{
    public struct SimulationTickState
    {
        public int HasMatchedInitiator;
        public int HasResolvedParticipants;
        public int LastMatchedInitiatorHasOutgoingRelations;
        public int LastScore;
        public int HasSelectedEvent;
        public int HasAppliedOutcome;
        public int ResolvedEventCount;
        public int AppliedOutcomeCount;

        public EntityId LastMatchedInitiator;
        public RelationContext LastResolvedRelationContext;

        public void ResetForSimulation()
        {
            this = default;
            LastMatchedInitiator = EntityId.Invalid;
            LastResolvedRelationContext = default;
        }

        public void ResetPerTick()
        {
            HasMatchedInitiator = 0;
            HasResolvedParticipants = 0;
            LastMatchedInitiatorHasOutgoingRelations = 0;
            LastMatchedInitiator = EntityId.Invalid;
            LastResolvedRelationContext = default;
            LastScore = 0;
            HasSelectedEvent = 0;
            HasAppliedOutcome = 0;
        }
    }
}
