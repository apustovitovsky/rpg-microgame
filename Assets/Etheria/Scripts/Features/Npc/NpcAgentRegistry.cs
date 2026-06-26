using System;
using System.Collections.Generic;
using Etheria.Game.Npc;
using UnityEngine;

namespace Etheria.Npc
{
    public sealed class NpcAgentRegistry :
        INpcAgentRegistry,
        INpcAgentRegistryWriter
    {
        private readonly Dictionary<string, INpcAgent> _agents =
            new(StringComparer.Ordinal);

        public bool TryGet(
            string npcId,
            out INpcAgent agent)
        {
            if (string.IsNullOrWhiteSpace(npcId))
            {
                agent = null;
                return false;
            }

            return _agents.TryGetValue(
                npcId,
                out agent);
        }

        public void Register(INpcAgent agent)
        {
            if (agent == null)
                return;

            string npcId = agent.NpcId;

            if (string.IsNullOrWhiteSpace(npcId))
            {
                Debug.LogError("Cannot register NPC agent with empty ID.");
                return;
            }

            if (_agents.TryGetValue(npcId, out var existing) &&
                !ReferenceEquals(existing, agent))
            {
                Debug.LogError(
                    $"Duplicate NPC agent registration for ID '{npcId}'.");

                return;
            }

            _agents[npcId] = agent;
        }

        public void Unregister(INpcAgent agent)
        {
            if (agent == null ||
                string.IsNullOrWhiteSpace(agent.NpcId))
            {
                return;
            }

            if (_agents.TryGetValue(agent.NpcId, out var existing) &&
                ReferenceEquals(existing, agent))
            {
                _agents.Remove(agent.NpcId);
            }
        }
    }
}