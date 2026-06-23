using System;
using System.Collections.Generic;
using Etheria.Game.Character;
using Etheria.Game.Dialogue;
using VContainer;
using Yarn.Unity;

namespace Etheria.Features.Campaign
{
    public sealed class YarnDialoguePresenter :
        DialoguePresenterBase
    {
        private DialoguePresenter _presenter;
        private IDialogueService _dialogueService;
        private ICharacterNameProvider _nameProvider;

        [Inject]
        public void Construct(
            DialoguePresenter presenter,
            IDialogueService dialogueService,
            ICharacterNameProvider nameProvider)
        {
            _presenter = presenter;
            _dialogueService = dialogueService;
            _nameProvider = nameProvider;
        }

        public override YarnTask OnDialogueStartedAsync()
        {
            _presenter.Begin();
            return YarnTask.CompletedTask;
        }

        public override async YarnTask RunLineAsync(
            LocalizedLine line,
            LineCancellationToken token)
        {
            string speakerName = GetSpeakerName(line);
            string text = line.TextWithoutCharacterName.Text;

            try
            {
                await _presenter.ShowLineAsync(
                    speakerName,
                    text);
            }
            catch (OperationCanceledException)
            {
                // Dialogue can be cancelled while a line is waiting for input.
            }
        }

        public override async YarnTask<DialogueOption> RunOptionsAsync(
            DialogueOption[] dialogueOptions,
            LineCancellationToken token)
        {
            var availableOptions =
                new List<DialogueOption>();

            var viewModels =
                new List<DialogueOptionViewModel>();

            foreach (DialogueOption option in dialogueOptions)
            {
                if (!option.IsAvailable)
                    continue;

                availableOptions.Add(option);

                viewModels.Add(
                    new DialogueOptionViewModel(
                        option.Line.TextWithoutCharacterName.Text,
                        true,
                        null));
            }

            if (availableOptions.Count == 0)
                return null;

            int selectedIndex =
                await _presenter.ShowOptionsAsync(viewModels);

            return availableOptions[selectedIndex];
        }

        public override YarnTask OnDialogueCompleteAsync()
        {
            _presenter.End();
            return YarnTask.CompletedTask;
        }

        private string GetSpeakerName(LocalizedLine line)
        {
            if (line.CharacterName == "player")
                return string.Empty;

            string speakerId =
                string.IsNullOrWhiteSpace(line.CharacterName)
                    ? _dialogueService.DefaultSpeakerId
                    : line.CharacterName;

            return string.IsNullOrWhiteSpace(speakerId)
                ? string.Empty
                : _nameProvider.GetDisplayName(speakerId);
        }
    }
}
