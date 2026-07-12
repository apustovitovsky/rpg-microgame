using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core;
using Game.World;

namespace Game.CommandSystem
{
    public sealed class WorldCommandReceiver :
        ICommandReceiver,
        IDisposable
    {
        private readonly IWorldInstance _instance;
        private readonly IRegistryWriter<ICommandReceiver> _receivers;

        private readonly Dictionary<Type, IWorldCommandHandler> _handlers =
            new();

        private bool _isExecuting;

        public WorldCommandReceiver(
            IWorldInstance instance,
            IEnumerable<IWorldCommandHandler> handlers,
            IRegistryWriter<ICommandReceiver> receivers)
        {
            _instance = instance
                ?? throw new ArgumentNullException(nameof(instance));

            _receivers = receivers
                ?? throw new ArgumentNullException(nameof(receivers));

            if (_instance.InstanceId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Instance id is required.",
                    nameof(instance));
            }

            if (handlers != null)
            {
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

            _receivers.Add(
                _instance.InstanceId,
                this);
        }

        public async UniTask<CommandResult> ReceiveAsync(
            IWorldCommand command,
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
            catch
            {
                return CommandResult.Failed;
            }
            finally
            {
                _isExecuting = false;
            }
        }

        public void Dispose()
        {
            _receivers.Remove(
                _instance.InstanceId,
                this);
        }
    }
}