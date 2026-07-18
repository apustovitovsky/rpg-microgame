using System;
using System.Collections.Generic;
using Game.Actor;

namespace Game.Gameplay
{
    public sealed class ActorRuntimeRegistry :
        IActorRuntimeRegistry
    {
        private readonly Dictionary<Guid, ActorRuntime>
            _runtimes = new();

        public void Register(ActorRuntime runtime)
        {
            if (runtime == null)
                throw new ArgumentNullException(nameof(runtime));

            if (!_runtimes.TryAdd(
                    runtime.InstanceId,
                    runtime))
            {
                throw new InvalidOperationException(
                    $"Actor runtime for instance " +
                    $"'{runtime.InstanceId}' is already registered.");
            }
        }

        public bool TryGet(
            Guid instanceId,
            out ActorRuntime runtime)
        {
            runtime = null;

            return instanceId != Guid.Empty &&
                   _runtimes.TryGetValue(
                       instanceId,
                       out runtime);
        }

        public bool Unregister(Guid instanceId)
        {
            return instanceId != Guid.Empty &&
                   _runtimes.Remove(instanceId);
        }
    }
}