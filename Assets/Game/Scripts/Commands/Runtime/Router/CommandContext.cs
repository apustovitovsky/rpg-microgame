using System;
using System.Threading;

namespace Game.Commands
{
    public readonly struct CommandContext
    {
        public CommandContext(
            Guid receiverId,
            CancellationToken cancellationToken)
        {
            ReceiverId = receiverId;
            CancellationToken = cancellationToken;
        }

        public Guid ReceiverId { get; }

        public CancellationToken CancellationToken { get; }
    }
}