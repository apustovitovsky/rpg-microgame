using System;
using System.Collections.Generic;
using Etheria.Game.Npc;

namespace Etheria.Npc
{
    public sealed class NpcStateRegistry :
        INpcStateRegistry
    {
        private readonly Dictionary<string, NpcState> _states = new(
            StringComparer.Ordinal);

        public NpcState GetOrCreate(
            string npcId)
        {
            npcId = Normalize(npcId);

            if (string.IsNullOrWhiteSpace(npcId))
                throw new ArgumentException(
                    "NPC id is empty.",
                    nameof(npcId));

            if (_states.TryGetValue(npcId, out var state))
                return state;

            state = new NpcState();
            _states.Add(npcId, state);

            return state;
        }

        public bool TryGet(
            string npcId,
            out NpcState state)
        {
            return _states.TryGetValue(
                Normalize(npcId),
                out state);
        }

        private static string Normalize(
            string value)
        {
            return value?.Trim() ?? string.Empty;
        }
    }
}