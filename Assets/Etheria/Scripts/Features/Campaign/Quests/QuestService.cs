using System;
using System.Collections.Generic;
using Etheria.Game.Quests;
using UnityEngine;

namespace Etheria.Features.Campaign
{
    public sealed class QuestService : IQuestService
    {
        private sealed class RuntimeQuestState
        {
            public QuestStatus Status;
            public int Stage;
            public readonly List<string> JournalEntries = new();
        }

        private readonly Dictionary<string, QuestDefinitionSO> _definitions;
        private readonly Dictionary<string, RuntimeQuestState> _states = new();

        public event Action<string> QuestChanged;

        public IReadOnlyList<string> GetTrackedQuestIds()
        {
            var result = new List<string>();

            foreach (var pair in _states)
            {
                if (pair.Value.Status != QuestStatus.Inactive)
                    result.Add(pair.Key);
            }

            return result.AsReadOnly();
        }

        public QuestService(QuestDefinitionSO[] definitions)
        {
            _definitions = new Dictionary<string, QuestDefinitionSO>(
                StringComparer.Ordinal);

            foreach (var definition in definitions)
            {
                if (definition == null)
                    throw new InvalidOperationException(
                        "Quest definition cannot be null.");

                if (string.IsNullOrWhiteSpace(definition.Id))
                    throw new InvalidOperationException(
                        $"Quest definition '{definition.name}' has no ID.");

                if (!_definitions.TryAdd(definition.Id, definition))
                    throw new InvalidOperationException(
                        $"Duplicate quest ID: '{definition.Id}'.");
            }
        }

        public QuestState GetState(string questId)
        {
            ValidateQuestId(questId);

            if (!_states.TryGetValue(questId, out var state))
            {
                state = new RuntimeQuestState();
                _states.Add(questId, state);
            }

            return new QuestState(
                state.Status,
                state.Stage,
                state.JournalEntries);
        }

        private void ValidateQuestId(string questId)
        {
            if (string.IsNullOrWhiteSpace(questId))
            {
                throw new ArgumentException(
                    "Quest ID cannot be empty.",
                    nameof(questId));
            }

            if (!_definitions.ContainsKey(questId))
            {
                throw new KeyNotFoundException(
                    $"Quest definition '{questId}' was not registered.");
            }
        }

        public bool TryStart(string questId)
        {
            if (!TryGetValidState(questId, out var state) ||
                state.Status != QuestStatus.Inactive)
            {
                return false;
            }

            state.Status = QuestStatus.Active;
            NotifyChanged(questId);
            return true;
        }

        public bool TrySetStage(string questId, int stage)
        {
            if (!TryGetValidState(questId, out var state) ||
                state.Status != QuestStatus.Active ||
                stage <= state.Stage ||
                !_definitions[questId].ContainsStage(stage))
            {
                return false;
            }

            state.Stage = stage;
            NotifyChanged(questId);
            return true;
        }

        public bool TryAddJournalEntry(string questId, string text)
        {
            if (!TryGetValidState(questId, out var state) ||
                state.Status != QuestStatus.Active ||
                string.IsNullOrWhiteSpace(text) ||
                state.JournalEntries.Contains(text))
            {
                return false;
            }

            state.JournalEntries.Add(text);
            NotifyChanged(questId);
            return true;
        }

        public bool TryComplete(string questId)
        {
            return TryFinish(questId, QuestStatus.Completed);
        }

        public bool TryFail(string questId)
        {
            return TryFinish(questId, QuestStatus.Failed);
        }

        private bool TryFinish(string questId, QuestStatus result)
        {
            if (!TryGetValidState(questId, out var state) ||
                state.Status != QuestStatus.Active)
            {
                return false;
            }

            state.Status = result;
            NotifyChanged(questId);
            return true;
        }

        private bool TryGetValidState(
            string questId,
            out RuntimeQuestState state)
        {
            if (string.IsNullOrWhiteSpace(questId) ||
                !_definitions.ContainsKey(questId))
            {
                state = null;
                return false;
            }

            if (!_states.TryGetValue(questId, out state))
            {
                state = new RuntimeQuestState();
                _states.Add(questId, state);
            }

            return true;
        }

        private void NotifyChanged(string questId)
        {
            QuestChanged?.Invoke(questId);

            var state = GetState(questId);

            Debug.Log(
                $"Quest '{questId}': " +
                $"status={state.Status}, " +
                $"stage={state.Stage}, " +
                $"journal entries={state.JournalEntries.Count}");
        }
    }
}