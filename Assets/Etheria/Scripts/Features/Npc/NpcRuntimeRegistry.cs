using System;
using System.Collections.Generic;
using Etheria.Game.Npc;
using UnityEngine;

namespace Etheria.Npc
{
    public sealed class NpcRuntimeRegistry :
        INpcRuntimeRegistry,
        INpcRuntimeRegistryWriter
    {
        private readonly Dictionary<string, INpcRuntime> _runtimes =
            new(StringComparer.Ordinal);

        public bool TryGet(
            string npcId,
            out INpcRuntime runtime)
        {
            if (string.IsNullOrWhiteSpace(npcId))
            {
                runtime = null;
                return false;
            }

            return _runtimes.TryGetValue(
                npcId,
                out runtime);
        }

        public void Register(INpcRuntime runtime)
        {
            if (runtime == null)
                return;

            var npcId = runtime.NpcId;

            if (string.IsNullOrWhiteSpace(npcId))
            {
                Debug.LogError("Cannot register NPC runtime with empty ID.");
                return;
            }

            if (_runtimes.TryGetValue(npcId, out var existing) &&
                !ReferenceEquals(existing, runtime))
            {
                Debug.LogError(
                    $"Duplicate NPC runtime registration for ID '{npcId}'.");

                return;
            }

            _runtimes[npcId] = runtime;
        }

        public void Unregister(INpcRuntime runtime)
        {
            if (runtime == null ||
                string.IsNullOrWhiteSpace(runtime.NpcId))
            {
                return;
            }

            if (_runtimes.TryGetValue(runtime.NpcId, out var existing) &&
                ReferenceEquals(existing, runtime))
            {
                _runtimes.Remove(runtime.NpcId);
            }
        }
    }
}