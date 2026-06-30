using System;
using Etheria.Game.Npc;
using Etheria.Game.Quests;
using UnityEngine;
using VContainer.Unity;
using Yarn.Unity;

namespace Etheria.Features.Campaign
{
    public sealed class QuestCommandHandler : IStartable, IDisposable
    {
        private readonly DialogueRunner _runner;
        private readonly IQuestService _questService;
        private readonly ICampaignQuestDefinitionProvider _definitions;
        private readonly INpcTravelService _travel;

        public QuestCommandHandler(
            DialogueRunner runner,
            IQuestService questService,
            ICampaignQuestDefinitionProvider definitions,
            INpcTravelService travel)
        {
            _runner = runner;
            _questService = questService;
            _definitions = definitions;
            _travel = travel;
        }

        public void Start()
        {
            _runner.AddCommandHandler<string>(
                "start_quest",
                OnStartQuest);

            _runner.AddCommandHandler<string, int>(
                "set_quest_stage",
                OnSetQuestStage);

            _runner.AddCommandHandler<string, string>(
                "add_quest_log",
                OnAddQuestLog);

            _runner.AddCommandHandler<string>(
                "complete_quest",
                OnCompleteQuest);

            _runner.AddCommandHandler<string>(
                "fail_quest",
                OnFailQuest);

            _runner.AddFunction<string, int>(
                "quest_stage",
                GetQuestStage);

            _runner.AddFunction<string, bool>(
                "quest_is_active",
                IsQuestActive);

            _runner.AddFunction<string, bool>(
                "quest_is_completed",
                IsQuestCompleted);

            _runner.AddFunction<string, bool>(
                "quest_is_failed",
                IsQuestFailed);

            _runner.AddCommandHandler<string, string>(
                "run_quest_travel",
                OnRunQuestTravel);
        }

        private void OnStartQuest(string questId)
        {
            _questService.TryStart(questId);
        }

        private void OnSetQuestStage(string questId, int stage)
        {
            _questService.TrySetStage(questId, stage);
        }

        private void OnAddQuestLog(string questId, string text)
        {
            _questService.TryAddJournalEntry(questId, text);
        }

        private void OnCompleteQuest(string questId)
        {
            _questService.TryComplete(questId);
        }

        private void OnFailQuest(string questId)
        {
            _questService.TryFail(questId);
        }

        private int GetQuestStage(string questId)
        {
            return _questService.GetState(questId).Stage;
        }

        private bool IsQuestActive(string questId)
        {
            return GetStatus(questId) == QuestStatus.Active;
        }

        private bool IsQuestCompleted(string questId)
        {
            return GetStatus(questId) == QuestStatus.Completed;
        }

        private bool IsQuestFailed(string questId)
        {
            return GetStatus(questId) == QuestStatus.Failed;
        }

        private void OnRunQuestTravel(
            string questId,
            string instructionId)
        {
            if (!_definitions.TryGetTravelInstruction(
                    questId,
                    instructionId,
                    out var instruction))
            {
                Debug.LogWarning(
                    $"Quest travel instruction '{questId}/{instructionId}' was not found.");
                return;
            }

            _travel.TrySendToAnchor(
                instruction.NpcId,
                instruction.LocationId,
                instruction.AnchorKey,
                instruction.QueryFilter);
        }

        private QuestStatus GetStatus(string questId)
        {
            return _questService.GetState(questId).Status;
        }

        public void Dispose()
        {
            _runner.RemoveCommandHandler("start_quest");
            _runner.RemoveCommandHandler("set_quest_stage");
            _runner.RemoveCommandHandler("add_quest_log");
            _runner.RemoveCommandHandler("complete_quest");
            _runner.RemoveCommandHandler("fail_quest");

            _runner.RemoveFunction("quest_stage");
            _runner.RemoveFunction("quest_is_active");
            _runner.RemoveFunction("quest_is_completed");
            _runner.RemoveFunction("quest_is_failed");
            _runner.RemoveCommandHandler("run_quest_travel");
        }
    }
}