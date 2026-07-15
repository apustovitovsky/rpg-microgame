using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core;
using UnityEngine;

namespace Game.Commands
{
    public sealed class CommandReceiver :
        ICommandReceiver
    {
        private readonly IInstanceIdentity _identity;

        private readonly Dictionary<Type, ICommandHandler> _handlers =
            new();

        private bool _isExecuting;

        public CommandReceiver(
            IInstanceIdentity identity,
            IEnumerable<ICommandHandler> handlers)
        {
            _identity = identity
                ?? throw new ArgumentNullException(nameof(identity));

            if (_identity.InstanceId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Instance id is required.",
                    nameof(identity));
            }

            if (handlers == null)
                return;

            foreach (var handler in handlers)
            {
                if (handler == null)
                    continue;

                if (!_handlers.TryAdd(
                        handler.CommandType,
                        handler))
                {
                    throw new InvalidOperationException(
                        $"Multiple handlers for command " +
                        $"'{handler.CommandType.Name}' " +
                        "are registered on one receiver.");
                }
            }
        }

        public async UniTask<CommandResult> ReceiveAsync(
            ICommand command,
            CancellationToken token)
        {
            if (command == null)
                return CommandResult.Rejected;

            if (token.IsCancellationRequested)
                return CommandResult.Cancelled;

            if (_isExecuting)
                return CommandResult.Busy;

            if (!_handlers.TryGetValue(
                    command.GetType(),
                    out var handler))
            {
                return CommandResult.Unsupported;
            }

            _isExecuting = true;

            try
            {
                return await handler.HandleAsync(
                    command,
                    _identity.InstanceId,
                    token);
            }
            catch (OperationCanceledException)
                when (token.IsCancellationRequested)
            {
                return CommandResult.Cancelled;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                return CommandResult.Failed;
            }
            finally
            {
                _isExecuting = false;
            }
        }
    }
}