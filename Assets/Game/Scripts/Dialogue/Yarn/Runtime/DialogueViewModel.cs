using System;
using System.Collections.Generic;

namespace Game.Dialogue.Yarn
{
    public readonly struct DialogueOptionViewModel
    {
        public DialogueOptionViewModel(
            string text,
            bool isAvailable,
            Action selected)
        {
            Text = text;
            IsAvailable = isAvailable;
            Selected = selected;
        }

        public string Text { get; }

        public bool IsAvailable { get; }

        public Action Selected { get; }
    }

    public interface IDialogueView
    {
        void Show();

        void Hide();

        void SetLine(
            string speakerName,
            string text);

        void ShowOptions(
            IReadOnlyList<DialogueOptionViewModel> options);

        void ClearOptions();
    }
}