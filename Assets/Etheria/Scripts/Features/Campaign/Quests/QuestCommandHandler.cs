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

            _runner.AddCommandHandler<string>(
                "complete_quest",
                OnCompleteQuest);

            _runner.AddFunction<string, bool>(
                "quest_is_active",
                IsQuestActive);

            _runner.AddFunction<string, bool>(
                "quest_is_completed",
                IsQuestCompleted);
        }

        private void OnStartQuest(string questId)
        {
            _questService.TryStart(questId);
        }

        private void OnCompleteQuest(string questId)
        {
            _questService.TryComplete(questId);
        }

        private bool IsQuestActive(string questId)
        {
            return _questService.GetStatus(questId) == QuestStatus.Active;
        }

        private bool IsQuestCompleted(string questId)
        {
            return _questService.GetStatus(questId) == QuestStatus.Completed;
        }

        public void Dispose()
        {
            _runner.RemoveCommandHandler("start_quest");
            _runner.RemoveCommandHandler("complete_quest");

            _runner.RemoveFunction("quest_is_active");
            _runner.RemoveFunction("quest_is_completed");
        }
    }
}