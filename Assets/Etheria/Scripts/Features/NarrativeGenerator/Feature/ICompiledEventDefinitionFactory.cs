using Unity.Collections;

namespace Etheria.Features.NarrativeGeneration
{
    public interface ICompiledEventDefinitionFactory
    {
        CompiledEventDefinition CreateBanditsRaidFarm(Allocator allocator);
    }
}
