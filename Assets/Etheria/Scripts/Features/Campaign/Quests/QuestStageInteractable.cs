using Etheria.Game.Interaction;
using Etheria.Game.Quests;
using UnityEngine;
using VContainer;

namespace Etheria.Features.Campaign
{
    public sealed class QuestStageInteractable :
        MonoBehaviour,
        IInteractable
    {
        [SerializeField] private string _questId;
        [SerializeField] private int _requiredStage;
        [SerializeField] private int _nextStage;
        [SerializeField, TextArea] private string _journalEntry;

        private IQuestService _questService;

        public bool CanInteract
        {
            get
            {
                if (_questService == null)
                    return false;

                var state = _questService.GetState(_questId);

                return state.Status == QuestStatus.Active &&
                       state.Stage == _requiredStage;
            }
        }

        [Inject]
        public void Construct(IQuestService questService)
        {
            _questService = questService;
        }

        public void Interact()
        {
            if (!CanInteract)
                return;

            if (!_questService.TrySetStage(_questId, _nextStage))
                return;

            if (!string.IsNullOrWhiteSpace(_journalEntry))
            {
                _questService.TryAddJournalEntry(
                    _questId,
                    _journalEntry);
            }
        }
    }
}