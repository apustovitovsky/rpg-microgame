using System;

namespace Etheria.Features.Campaign
{
    public readonly struct DialogueOptionViewModel
    {
        public string Text { get; }
        public bool IsAvailable { get; }
        public Action Selected { get; }

        public DialogueOptionViewModel(
            string text,
            bool isAvailable,
            Action selected)
        {
            Text = text;
            IsAvailable = isAvailable;
            Selected = selected;
        }
    }
}