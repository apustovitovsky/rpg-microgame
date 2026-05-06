using Unity.Collections;

namespace Etheria.Features.NarrativeGeneration
{
    public struct WorldSimulationProcessor
    {
        public WorldState WorldState;

        [ReadOnly]
        public CompiledEventDefinition EventDefinition;

        public NativeReference<SimulationTickState> TickState;

        public void InitializeSimulation()
        {
            var tickState = TickState.Value;
            tickState.ResetForSimulation();
            TickState.Value = tickState;
        }

        public void RunTick()
        {
            ResetTickState();
            RunModifierPhase();
            RebuildRelationLookup();
            RunMatcher();
            RunEvaluator();
            RunSelection();
            ApplyOutcomes();
        }

        private void ResetTickState()
        {
            var tickState = TickState.Value;
            tickState.ResetPerTick();
            TickState.Value = tickState;
        }

        private readonly void RunModifierPhase()
        {
            // TODO: apply daily modifier changes to WorldState.
        }

        private void RebuildRelationLookup()
        {
            WorldState.RebuildRelationLookup();
        }

        private void RunMatcher()
        {
            var tickState = TickState.Value;

            if (!EventMatcher.TryMatchInitiator(WorldState, EventDefinition.InitiatorQuery, out var initiator))
            {
                TickState.Value = tickState;
                return;
            }

            tickState.HasMatchedInitiator = 1;
            tickState.LastMatchedInitiator = initiator;
            tickState.LastMatchedInitiatorHasOutgoingRelations =
                WorldState.RelationStore.HasOutgoingRelations(initiator) ? 1 : 0;

            if (!EventMatcher.TryMatchRelationChain(
                    WorldState,
                    EventDefinition,
                    initiator,
                    out var participantContext))
            {
                TickState.Value = tickState;
                return;
            }

            tickState.HasResolvedParticipants = 1;
            tickState.LastResolvedRelationContext = participantContext;
            tickState.ResolvedEventCount += 1;
            TickState.Value = tickState;
        }

        private void RunEvaluator()
        {
            var tickState = TickState.Value;
            if (tickState.HasResolvedParticipants == 0)
            {
                TickState.Value = tickState;
                return;
            }

            tickState.LastScore = EventEvaluator.Evaluate(
                WorldState,
                EventDefinition,
                tickState.LastResolvedRelationContext);

            TickState.Value = tickState;
        }

        private void RunSelection()
        {
            var tickState = TickState.Value;
            tickState.HasSelectedEvent = tickState.LastScore > 0 ? 1 : 0;
            TickState.Value = tickState;
        }

        private void ApplyOutcomes()
        {
            var tickState = TickState.Value;
            if (tickState.HasSelectedEvent == 0)
            {
                TickState.Value = tickState;
                return;
            }

            var raidTargetFarm = tickState.LastResolvedRelationContext.Slot1;
            if (!raidTargetFarm.IsValid)
            {
                TickState.Value = tickState;
                return;
            }

            WorldState.AddEntityTags(raidTargetFarm, PrototypeTags.Destroyed);
            tickState.HasAppliedOutcome = 1;
            tickState.AppliedOutcomeCount += 1;
            TickState.Value = tickState;
        }
    }
}
