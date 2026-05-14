using System;

namespace Etheria.Features.HWFC
{
    public sealed class WaveState
    {
        public WaveState(ModuleSet[] domains, int[] versions, MinPriorityQueue<EntropyQueueEntry> entropyQueue, int contradictionNodeIndex)
        {
            Domains = domains ?? throw new ArgumentNullException(nameof(domains));
            Versions = versions ?? throw new ArgumentNullException(nameof(versions));
            EntropyQueue = entropyQueue ?? throw new ArgumentNullException(nameof(entropyQueue));

            if (domains.Length != versions.Length)
            {
                throw new ArgumentException("Domain and version counts must match.");
            }

            ContradictionNodeIndex = contradictionNodeIndex;
        }

        public ModuleSet[] Domains { get; }

        public int[] Versions { get; }

        public MinPriorityQueue<EntropyQueueEntry> EntropyQueue { get; }

        public int ContradictionNodeIndex { get; }

        public bool HasContradiction => ContradictionNodeIndex >= 0;
    }
}
