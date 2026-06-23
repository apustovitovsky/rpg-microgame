using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Etheria.Game.Input;
using UnityEngine;

namespace Etheria.Features.Campaign
{
    public sealed class DialoguePresenter
    {
        private readonly DialogueView _view;
        private UniTaskCompletionSource _continueSource;
        private readonly IPlayerInputSource _input;

        public bool IsWaitingForContinue =>
            _continueSource != null;

        public DialoguePresenter(
            DialogueView view,
            IPlayerInputSource input)
        {
            _view = view;
            _input = input;
        }

        public bool TryContinue()
        {
            if (_continueSource == null)
                return false;

            _continueSource.TrySetResult();
            return true;
        }

        public void Begin()
        {
            _input.EnterUiInput();
            _view.ClearOptions();
            _view.Show();
        }

        public void End()
        {
            _continueSource?.TrySetCanceled();
            _continueSource = null;

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

            var selectionSource =
                new UniTaskCompletionSource<int>();

            var viewModels =
                new List<DialogueOptionViewModel>(options.Count);

            for (int index = 0; index < options.Count; index++)
            {
                int selectedIndex = index;
                DialogueOptionViewModel option = options[index];

                viewModels.Add(
                    new DialogueOptionViewModel(
                        option.Text,
                        option.IsAvailable,
                        () => selectionSource.TrySetResult(
                            selectedIndex)));
            }

            _view.ShowOptions(viewModels);

            int result = await selectionSource.Task;

            _view.ClearOptions();
            return result;
        }
    }
}
