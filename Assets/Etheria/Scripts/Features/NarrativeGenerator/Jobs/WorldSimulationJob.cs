using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Etheria.Features.NarrativeGeneration
{
    [BurstCompile]
    public struct WorldSimulationJob : IJob
    {
        public WorldSimulationProcessor Processor;
        public int DayCount;

        public void Execute()
        {
            Processor.InitializeSimulation();

            for (var dayIndex = 0; dayIndex < DayCount; dayIndex++)
            {
                Processor.RunTick();
            }
        }
    }
}
