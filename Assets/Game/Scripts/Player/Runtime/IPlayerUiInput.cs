using System;

namespace Game.Player
{
    public interface IPlayerUiInput
    {
        event Action UiSubmitPerformed;
        event Action UiCancelPerformed;

        void EnterGameplayInput();

        void EnterUiInput();
    }
}