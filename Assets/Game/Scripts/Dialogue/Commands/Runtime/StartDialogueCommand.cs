using System;
using Game.Commands;

namespace Game.Dialogue.Commands
{
    public readonly struct StartDialogueCommand :
        ICommand<DialogueStartResult>
    {
        public StartDialogueCommand(
            Guid initiatorInstanceId)
        {
            InitiatorInstanceId = initiatorInstanceId;
        }

        public Guid InitiatorInstanceId { get; }
    }
}