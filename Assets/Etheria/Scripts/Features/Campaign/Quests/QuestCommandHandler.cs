using System;
using Etheria.Game.Quests;
using VContainer.Unity;
using Yarn.Unity;

namespace Etheria.Features.Campaign
{
    public sealed class QuestCommandHandler : IStartable, IDisposable
    {
        private readonly DialogueRunner _runner;
        private readonly IQuestService _questService;

        public QuestCommandHandler(
            DialogueRunner runner,
            IQuestService questService)
        {
            _runner = runner;
            _questService = questService;
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
        }
    }
}