using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using Yarn.Unity;

namespace Game.Dialogue.Yarn
{
    [DisallowMultipleComponent]
    public sealed class YarnDialoguePresenter :
        DialoguePresenterBase
    {
        private DialoguePresenter _presenter;
        private DialogueRunner _runner;

        [Inject]
        public void Construct(
            DialoguePresenter presenter,
            DialogueRunner runner)
        {
            _presenter = presenter;
            _runner = runner;

            _presenter.CancelRequested += StopDialogue;
        }

        private void OnDestroy()
        {
            if (_presenter != null)
            {
                _presenter.CancelRequested -= StopDialogue;
            }
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
            try
            {
                await _presenter.ShowLineAsync(
                    GetSpeakerName(line),
                    line.TextWithoutCharacterName.Text);
            }
            catch (OperationCanceledException)
            {
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

            foreach (var option in dialogueOptions)
            {
                if (!option.IsAvailable)
                {
                    continue;
                }

                availableOptions.Add(option);

                viewModels.Add(
                    new DialogueOptionViewModel(
                        option.Line.TextWithoutCharacterName.Text,
                        true,
                        null));
            }

            if (availableOptions.Count == 0)
            {
                return null;
            }

            try
            {
                var selectedIndex =
                    await _presenter.ShowOptionsAsync(viewModels);

                return availableOptions[selectedIndex];
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }

        public override YarnTask OnDialogueCompleteAsync()
        {
            _presenter.End();

            return YarnTask.CompletedTask;
        }

        private void StopDialogue()
        {
            if (_runner.IsDialogueRunning)
            {
                _runner.Stop().Forget();
            }
        }

        private static string GetSpeakerName(
            LocalizedLine line)
        {
            return line.CharacterName == "player"
                ? string.Empty
                : line.CharacterName;
        }
    }
}