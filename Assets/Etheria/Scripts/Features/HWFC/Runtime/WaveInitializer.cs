using System;

namespace Etheria.Features.HWFC
{
    public static class WaveInitializer
    {
        public static WaveState CreateInitialState(GenerationProblem problem)
        {
            if (problem == null)
            {
                throw new ArgumentNullException(nameof(problem));
            }

            var nodeCount = problem.Topology.NodeCount;
            var moduleCount = problem.ModuleModel.ModuleCount;
            var domains = new ModuleSet[nodeCount];
            var versions = new int[nodeCount];
            var queue = new MinPriorityQueue<EntropyQueueEntry>();
            var contradictionNodeIndex = -1;

            for (var nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
            {
                var domain = problem.InitialDomains == null
                    ? problem.ModuleModel.Universe
                    : problem.InitialDomains[nodeIndex];

                if (domain.ModuleCount != moduleCount)
                {
                    throw new InvalidOperationException("Initial domain module count does not match the problem model.");
                }

                domains[nodeIndex] = domain;
                versions[nodeIndex] = 0;

                if (domain.IsEmpty)
                {
                    if (contradictionNodeIndex < 0)
                    {
                        contradictionNodeIndex = nodeIndex;
                    }

                    continue;
                }

                if (domain.Count <= 1)
                {
                    continue;
                }

                var entropy = domain.CalculateEntropy(problem.ModuleModel);
                queue.Enqueue(new EntropyQueueEntry(nodeIndex, versions[nodeIndex], entropy), entropy);
            }

            return new WaveState(domains, versions, queue, contradictionNodeIndex);
        }
    }
}
