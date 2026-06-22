using System;
using System.Collections.Generic;

namespace Etheria.Game.Quests
{
    public interface IQuestService
    {
        QuestState GetState(string questId);
        IReadOnlyList<string> GetTrackedQuestIds();

        bool TryStart(string questId);
        bool TrySetStage(string questId, int stage);
        bool TryAddJournalEntry(string questId, string text);
        bool TryComplete(string questId);
        bool TryFail(string questId);

        event Action<string> QuestChanged;
    }
}