using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core;

namespace Game.Commands
{
    public sealed class CommandManager :
        ICommandManager
    {
        private readonly IRegistry<ICommandReceiver> _receivers;

        public CommandManager(
            IRegistry<ICommandReceiver> receivers)
        {
            _receivers = receivers
                ?? throw new ArgumentNullException(nameof(receivers));
        }

        public UniTask<CommandResult> SendAsync(
            Guid targetInstanceId,
            IWorldCommand command,
            CancellationToken token)
        {
            if (targetInstanceId == Guid.Empty)
            {
                return UniTask.FromResult(
                    CommandResult.TargetNotFound);
            }

            if (command == null)
            {
                return UniTask.FromResult(
                    CommandResult.Rejected);
            }

            if (token.IsCancellationRequested)
            {
                return UniTask.FromResult(
                    CommandResult.Cancelled);
            }

            return _receivers.TryGet(
                    targetInstanceId,
                    out var receiver)
                ? receiver.ReceiveAsync(command, token)
                : UniTask.FromResult(
                    CommandResult.TargetNotFound);
        }
    }
}