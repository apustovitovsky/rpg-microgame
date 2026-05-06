using Unity.Collections;

namespace Etheria.Features.NarrativeGeneration
{
    public interface INarrativeGenerationService
    {
        WorldState RunTick(WorldState currentState, Allocator allocator);
        WorldState RunSimulation(WorldState initialState, int dayCount, Allocator allocator);
        WorldState RunTick(WorldState currentState, CompiledEventDefinition eventDefinition);
        WorldState RunSimulation(WorldState initialState, CompiledEventDefinition eventDefinition, int dayCount);
    }
}
