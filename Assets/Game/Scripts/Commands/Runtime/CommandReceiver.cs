using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.World;
using UnityEngine;

namespace Game.Commands
{
    public sealed class CommandReceiver :
        ICommandReceiver
    {
        private readonly WorldInstance _instance;

        private readonly Dictionary<Type, ICommandHandler> _handlers =
            new();

        private bool _isExecuting;

        public CommandReceiver(
            WorldInstance instance,
            IEnumerable<ICommandHandler> handlers)
        {
            _instance = instance
                ?? throw new ArgumentNullException(nameof(instance));

            if (_instance.InstanceId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Instance id is required.",
                    nameof(instance));
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
                    _instance.InstanceId,
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
