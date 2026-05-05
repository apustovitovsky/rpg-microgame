using System;
using Unity.Collections;
using Unity.Jobs;

namespace Etheria.Features.NarrativeGeneration
{
    public struct CompiledEventDefinition : IDisposable
    {
        public TagQuery InitiatorQuery;

        [ReadOnly]
        public NativeArray<RelationStep> RelationNodes;

        public readonly bool IsCreated => RelationNodes.IsCreated;

        public void Dispose()
        {
            if (RelationNodes.IsCreated)
            {
                RelationNodes.Dispose();
            }
        }

        public JobHandle Dispose(JobHandle dependency)
        {
            return RelationNodes.IsCreated
                ? RelationNodes.Dispose(dependency)
                : dependency;
        }
    }
}
