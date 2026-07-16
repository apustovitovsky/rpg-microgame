using System;
using Game.Commands;

namespace Game.Dialogue.Commands
{
    public readonly struct ExitDialogueCommand :
        ICommand
    {
        public ExitDialogueCommand(
            Guid sessionId)
        {
            SessionId = sessionId;
        }

        public Guid SessionId { get; }
    }
}