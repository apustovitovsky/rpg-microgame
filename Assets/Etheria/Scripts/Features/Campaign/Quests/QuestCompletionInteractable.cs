using Etheria.Game.Interaction;
using Etheria.Game.Quests;
using UnityEngine;
using VContainer;

namespace Etheria.Features.Campaign
{
    public sealed class QuestCompletionInteractable :
        MonoBehaviour,
        IInteractable
    {
        [SerializeField] private string _questId;

        private IQuestService _questService;

        public bool CanInteract =>
            _questService != null &&
            _questService.GetStatus(_questId) == QuestStatus.Active;

        [Inject]
        public void Construct(IQuestService questService)
        {
            _questService = questService;
        }

        public void Interact()
        {
            if (CanInteract)
                _questService.TryComplete(_questId);
        }
    }
}