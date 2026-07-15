using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core;

namespace Game.Commands
{
    public sealed class CommandDispatch :
        ICommandDispatch,
        IRegistryWriter<ICommandReceiver>
    {
        private readonly Dictionary<Guid, ICommandReceiver> _receivers =
            new();

        public void Add(
            Guid id,
            ICommandReceiver receiver)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException(
                    "Id is required.",
                    nameof(id));
            }

            if (receiver == null)
                throw new ArgumentNullException(nameof(receiver));

            if (!_receivers.TryAdd(id, receiver))
            {
                throw new InvalidOperationException(
                    $"Command receiver is already registered " +
                    $"for '{id}'.");
            }
        }

        public bool Remove(
            Guid id,
            ICommandReceiver expectedReceiver)
        {
            if (id == Guid.Empty ||
                expectedReceiver == null ||
                !_receivers.TryGetValue(
                    id,
                    out var receiver) ||
                !ReferenceEquals(receiver, expectedReceiver))
            {
                return false;
            }

            return _receivers.Remove(id);
        }

        public UniTask<CommandResult> SendAsync(
            Guid targetInstanceId,
            ICommand command,
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

            return _receivers.TryGetValue(
                    targetInstanceId,
                    out var receiver)
                ? receiver.ReceiveAsync(command, token)
                : UniTask.FromResult(
                    CommandResult.TargetNotFound);
        }
    }
}