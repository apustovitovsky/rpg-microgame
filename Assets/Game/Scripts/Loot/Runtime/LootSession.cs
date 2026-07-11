using System;

namespace Game.Loot
{
    public sealed class LootSession
    {
        public LootSession(
            Guid looterInstanceId,
            Guid sourceInstanceId)
        {
            if (looterInstanceId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Loot session looter id is required.",
                    nameof(looterInstanceId));
            }

            if (sourceInstanceId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Loot session source id is required.",
                    nameof(sourceInstanceId));
            }

            Id = Guid.NewGuid();
            LooterInstanceId = looterInstanceId;
            SourceInstanceId = sourceInstanceId;
        }

        public Guid Id { get; }

        public Guid LooterInstanceId { get; }

        public Guid SourceInstanceId { get; }
    }
}