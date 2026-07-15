using System;
using Game.Commands;

namespace Game.Dialogue.Commands
{
    // TODO: Register a handler when AI and scripted dialogue starts are added.
    public readonly struct StartDialogueCommand :
        ICommand
    {
        public StartDialogueCommand(
            Guid initiatorInstanceId)
        {
            InitiatorInstanceId = initiatorInstanceId;
        }

        public Guid InitiatorInstanceId { get; }
    }
}