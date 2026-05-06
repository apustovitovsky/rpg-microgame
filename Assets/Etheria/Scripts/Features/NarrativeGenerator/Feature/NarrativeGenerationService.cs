using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Etheria.Features.NarrativeGeneration
{
    public sealed class NarrativeGenerationService : INarrativeGenerationService
    {
        private readonly ICompiledEventDefinitionFactory _eventDefinitionFactory;

        public NarrativeGenerationService(ICompiledEventDefinitionFactory eventDefinitionFactory)
        {
            _eventDefinitionFactory = eventDefinitionFactory;
        }

        public WorldState RunTick(WorldState currentState, Allocator allocator)
        {
            return RunSimulation(currentState, 1, allocator);
        }

        public WorldState RunSimulation(WorldState initialState, int dayCount, Allocator allocator)
        {
            var eventDefinition = _eventDefinitionFactory.CreateBanditsRaidFarm(allocator);

            try
            {
                return RunSimulation(initialState, eventDefinition, dayCount);
            }
            finally
            {
                eventDefinition.Dispose();
            }
        }

        public WorldState RunTick(WorldState currentState, CompiledEventDefinition eventDefinition)
        {
            return RunSimulation(currentState, eventDefinition, 1);
        }

        public WorldState RunSimulation(WorldState initialState, CompiledEventDefinition eventDefinition, int dayCount)
        {
            var tickState = new NativeReference<SimulationTickState>(Allocator.TempJob);

            var processor = new WorldSimulationProcessor
            {
                WorldState = initialState,
                EventDefinition = eventDefinition,
                TickState = tickState
            };

            var job = new WorldSimulationJob
            {
                Processor = processor,
                DayCount = dayCount
            };

            try
            {
                job.Run();
                var state = tickState.Value;

                if (state.HasResolvedParticipants != 0)
                {
                    var resolvedContext = state.LastResolvedRelationContext;
                    Debug.Log(
                        $"Narrative generation resolved event participants. " +
                        $"Initiator={state.LastMatchedInitiator.Value}, " +
                        $"Slot1={resolvedContext.Slot1.Value}, " +
                        $"Slot2={resolvedContext.Slot2.Value}, " +
                        $"Score={state.LastScore}, " +
                        $"Selected={state.HasSelectedEvent != 0}, " +
                        $"OutcomeApplied={state.HasAppliedOutcome != 0}, " +
                        $"ResolvedCount={state.ResolvedEventCount}, " +
                        $"AppliedOutcomeCount={state.AppliedOutcomeCount}");
                }
                else
                {
                    Debug.Log(
                        $"Narrative generation did not resolve any event participants. " +
                        $"HasMatchedInitiator={state.HasMatchedInitiator != 0}, " +
                        $"Initiator={state.LastMatchedInitiator.Value}, " +
                        $"HasOutgoingRelations={state.LastMatchedInitiatorHasOutgoingRelations != 0}, " +
                        $"ResolvedCount={state.ResolvedEventCount}, " +
                        $"AppliedOutcomeCount={state.AppliedOutcomeCount}");
                }

                return initialState;
            }
            finally
            {
                tickState.Dispose();
            }
        }
    }
}
