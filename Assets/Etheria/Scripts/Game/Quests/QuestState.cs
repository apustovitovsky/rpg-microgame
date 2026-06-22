using System.Collections.Generic;

namespace Etheria.Game.Quests
{
    public sealed class QuestState
    {
        public QuestStatus Status { get; }
        public int Stage { get; }
        public IReadOnlyList<string> JournalEntries { get; }

        public QuestState(
            QuestStatus status,
            int stage,
            IEnumerable<string> journalEntries)
        {
            Status = status;
            Stage = stage;
            JournalEntries =
                new List<string>(journalEntries).AsReadOnly();
        }
    }
}