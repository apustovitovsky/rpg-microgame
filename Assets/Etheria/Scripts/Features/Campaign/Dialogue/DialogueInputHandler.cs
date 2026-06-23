using System;
using Etheria.Game.Input;
using VContainer.Unity;

namespace Etheria.Features.Campaign
{
    public sealed class DialogueInputHandler :
        IStartable,
        IDisposable
    {
        private readonly IPlayerInputSource _input;
        private readonly DialoguePresenter _presenter;

        public DialogueInputHandler(
            IPlayerInputSource input,
            DialoguePresenter presenter)
        {
            _input = input;
            _presenter = presenter;
        }

        public void Start()
        {
            _input.UiSubmitPerformed += OnSubmit;
        }

        public void Dispose()
        {
            _input.UiSubmitPerformed -= OnSubmit;
        }

        private void OnSubmit()
        {
            _presenter.TryContinue();
        }
    }
}
