using System.Collections.Generic;
using Etheria.Game.Quests;
using UnityEngine;

namespace Etheria.Features.Campaign
{
    public sealed class QuestService : IQuestService
    {
        private readonly Dictionary<string, QuestStatus> _statuses = new();

        public bool TryStart(string questId)
        {
            if (string.IsNullOrWhiteSpace(questId) ||
                GetStatus(questId) != QuestStatus.Inactive)
            {
                return false;
            }

            _statuses[questId] = QuestStatus.Active;
            Debug.Log($"Quest started: {questId}");
            return true;
        }

        public bool TryComplete(string questId)
        {
            if (string.IsNullOrWhiteSpace(questId) ||
                GetStatus(questId) != QuestStatus.Active)
            {
                return false;
            }

            _statuses[questId] = QuestStatus.Completed;
            Debug.Log($"Quest completed: {questId}");
            return true;
        }

        public QuestStatus GetStatus(string questId)
        {
            return _statuses.TryGetValue(questId, out var status)
                ? status
                : QuestStatus.Inactive;
        }
    }
}