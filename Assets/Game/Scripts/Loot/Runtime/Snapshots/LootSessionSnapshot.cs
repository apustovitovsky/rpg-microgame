using System;
using System.Collections.Generic;

namespace Game.Loot
{
    public readonly struct LootEntrySnapshot
    {
        public LootEntrySnapshot(
            Guid itemInstanceId,
            string itemDefinitionId,
            int count)
        {
            if (itemInstanceId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Loot entry item instance id is required.",
                    nameof(itemInstanceId));
            }

            if (string.IsNullOrWhiteSpace(itemDefinitionId))
            {
                throw new ArgumentException(
                    "Loot entry item definition id is required.",
                    nameof(itemDefinitionId));
            }

            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            ItemInstanceId = itemInstanceId;
            ItemDefinitionId = itemDefinitionId;
            Count = count;
        }

        public Guid ItemInstanceId { get; }

        public string ItemDefinitionId { get; }

        public int Count { get; }
    }

    public readonly struct LootSessionSnapshot
    {
        public LootSessionSnapshot(
            Guid sessionId,
            LootEntrySnapshot[] entries)
        {
            if (sessionId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Loot session id is required.",
                    nameof(sessionId));
            }

            if (entries == null)
                throw new ArgumentNullException(nameof(entries));

            SessionId = sessionId;
            Entries = Array.AsReadOnly(entries);
        }

        public Guid SessionId { get; }

        public IReadOnlyList<LootEntrySnapshot> Entries { get; }
    }
}