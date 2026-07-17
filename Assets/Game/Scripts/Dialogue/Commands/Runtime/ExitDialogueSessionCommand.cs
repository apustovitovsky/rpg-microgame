using System;
using Game.Commands;

namespace Game.Dialogue.Commands
{
    public readonly struct ExitDialogueSessionCommand :
        ICommand
    {
        public ExitDialogueSessionCommand(
            Guid sessionId)
        {
            SessionId = sessionId;
        }

        public Guid SessionId { get; }
    }
}