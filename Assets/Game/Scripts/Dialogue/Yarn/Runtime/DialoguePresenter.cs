using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Player;

namespace Game.Dialogue.Yarn
{
    public sealed class DialoguePresenter
    {
        private readonly IDialogueView _view;
        private readonly IPlayerUiInput _input;

        private UniTaskCompletionSource _continueSource;
        private UniTaskCompletionSource<int> _selectionSource;

        public event Action CancelRequested;

        public DialoguePresenter(
            IDialogueView view,
            IPlayerUiInput input)
        {
            _view = view;
            _input = input;
        }

        public void Begin()
        {
            _input.UiSubmitPerformed += TryContinue;
            _input.UiCancelPerformed += RequestCancel;
            _input.EnterUiInput();

            _view.ClearOptions();
            _view.Show();
        }

        public void End()
        {
            _continueSource?.TrySetCanceled();
            _continueSource = null;

            _selectionSource?.TrySetCanceled();
            _selectionSource = null;

            _input.UiSubmitPerformed -= TryContinue;
            _input.UiCancelPerformed -= RequestCancel;
            _input.EnterGameplayInput();

            _view.Hide();
        }

        public async UniTask ShowLineAsync(
            string speakerName,
            string text)
        {
            _view.ClearOptions();
            _view.SetLine(speakerName, text);

            _continueSource = new UniTaskCompletionSource();

            await _continueSource.Task;

            _continueSource = null;
        }

        public async UniTask<int> ShowOptionsAsync(
            IReadOnlyList<DialogueOptionViewModel> options)
        {
            _continueSource = null;
            _selectionSource = new UniTaskCompletionSource<int>();

            var boundOptions =
                new List<DialogueOptionViewModel>(options.Count);

            for (var index = 0; index < options.Count; index++)
            {
                var selectedIndex = index;
                var option = options[index];

                boundOptions.Add(
                    new DialogueOptionViewModel(
                        option.Text,
                        option.IsAvailable,
                        () => _selectionSource.TrySetResult(
                            selectedIndex)));
            }

            _view.ShowOptions(boundOptions);

            var selected = await _selectionSource.Task;

            _selectionSource = null;
            _view.ClearOptions();

            return selected;
        }

        private void TryContinue()
        {
            _continueSource?.TrySetResult();
        }

        private void RequestCancel()
        {
            CancelRequested?.Invoke();
        }
    }
}