using System;
using Etheria.Game.Input;
using Etheria.Game.Quests;
using VContainer.Unity;

namespace Etheria.Features.Campaign
{
    public sealed class QuestJournalPresenter :
        IStartable,
        IDisposable
    {
        private readonly IQuestService _questService;
        private readonly IQuestTextProvider _textProvider;
        private readonly QuestJournalView _view;

        private readonly IPlayerInputSource _inputSource;
        private bool _isOpen;

        public QuestJournalPresenter(
            IQuestService questService,
            IQuestTextProvider textProvider,
            QuestJournalView view,
            IPlayerInputSource inputSource)
        {
            _questService = questService;
            _textProvider = textProvider;
            _view = view;
            _inputSource = inputSource;
        }

        public void Start()
        {
            _questService.QuestChanged += OnQuestChanged;
            _inputSource.ToggleJournalPerformed += Toggle;

            _view.Hide();
        }

        public void Dispose()
        {
            _questService.QuestChanged -= OnQuestChanged;
            _inputSource.ToggleJournalPerformed -= Toggle;
        }

        private void Toggle()
        {
            _isOpen = !_isOpen;

            if (_isOpen)
                Refresh();
            else
                _view.Hide();
        }

        private void OnQuestChanged(string questId)
        {
            if (_isOpen)
                Refresh();
        }

        private void Refresh()
        {
            var questIds = _questService.GetTrackedQuestIds();

            if (questIds.Count == 0)
            {
                _isOpen = false;
                _view.Hide();
                return;
            }

            var questId = questIds[questIds.Count - 1];
            var state = _questService.GetState(questId);

            var localizedEntries =
                new string[state.JournalEntries.Count];

            for (var i = 0; i < localizedEntries.Length; i++)
            {
                localizedEntries[i] =
                    _textProvider.GetText(state.JournalEntries[i]);
            }

            _view.Show(
                _textProvider.GetText($"{questId}.title"),
                string.Join("\n\n", localizedEntries));
        }
    }
}