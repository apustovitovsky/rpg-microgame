using Etheria.Game.Character;
using Etheria.Game.Dialogue;
using TMPro;
using UnityEngine;
using VContainer;
using Yarn.Unity;

namespace Etheria.Features.Campaign
{
    public sealed class DialogueSpeakerPresenter :
        DialoguePresenterBase
    {
        [SerializeField] private TMP_Text _speakerName;
        [SerializeField] private GameObject _speakerNameContainer;

        private IDialogueService _dialogueService;

        private ICharacterNameProvider _characterNameProvider;

        [Inject]
        public void Construct(
            IDialogueService dialogueService,
            ICharacterNameProvider characterNameProvider)
        {
            _dialogueService = dialogueService;
            _characterNameProvider = characterNameProvider;
        }

        public override YarnTask OnDialogueStartedAsync()
        {
            _speakerNameContainer.SetActive(false);
            return YarnTask.CompletedTask;
        }

        public override YarnTask RunLineAsync(
            LocalizedLine line,
            LineCancellationToken token)
        {
            if (line.CharacterName == "player")
            {
                _speakerNameContainer.SetActive(false);
                return YarnTask.CompletedTask;
            }

            var speakerId = string.IsNullOrWhiteSpace(line.CharacterName)
                ? _dialogueService.DefaultSpeakerId
                : line.CharacterName;

            var hasSpeaker = !string.IsNullOrWhiteSpace(speakerId);

            _speakerNameContainer.SetActive(hasSpeaker);

            if (hasSpeaker)
                _speakerName.text = _characterNameProvider.GetDisplayName(speakerId);

            return YarnTask.CompletedTask;
        }

        public override YarnTask OnDialogueCompleteAsync()
        {
            _speakerNameContainer.SetActive(false);
            return YarnTask.CompletedTask;
        }
    }
}