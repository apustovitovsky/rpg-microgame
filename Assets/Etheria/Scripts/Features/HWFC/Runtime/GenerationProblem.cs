using System;

namespace Etheria.Features.HWFC
{
    public sealed class GenerationProblem
    {
        public GenerationProblem(ModuleModel moduleModel, Topology topology, ModuleSet[] initialDomains, int seed)
        {
            ModuleModel = moduleModel ?? throw new ArgumentNullException(nameof(moduleModel));
            Topology = topology ?? throw new ArgumentNullException(nameof(topology));

            if (!moduleModel.DirectionModel.IsEquivalentTo(topology.DirectionModel))
            {
                throw new ArgumentException("ModuleModel and Topology must use equivalent direction models.");
            }

            if (initialDomains != null && initialDomains.Length != topology.NodeCount)
            {
                throw new ArgumentException("Initial domain count must match topology node count.", nameof(initialDomains));
            }

            if (initialDomains == null)
            {
                InitialDomains = null;
            }
            else
            {
                InitialDomains = new ModuleSet[initialDomains.Length];
                Array.Copy(initialDomains, InitialDomains, initialDomains.Length);

                for (var nodeIndex = 0; nodeIndex < InitialDomains.Length; nodeIndex++)
                {
                    if (InitialDomains[nodeIndex].ModuleCount != moduleModel.ModuleCount)
                    {
                        throw new ArgumentException("Every initial domain must use the model module count.", nameof(initialDomains));
                    }
                }
            }

            Seed = seed;
        }

        public ModuleModel ModuleModel { get; }

        public Topology Topology { get; }

        public ModuleSet[] InitialDomains { get; }

        public int Seed { get; }
    }
}
