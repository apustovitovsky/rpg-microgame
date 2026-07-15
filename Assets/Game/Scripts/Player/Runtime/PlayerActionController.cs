using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Commands;
using Game.Interaction;
using VContainer.Unity;

namespace Game.Player
{
    public sealed class PlayerActionController :
        IStartable,
        IDisposable
    {
        private readonly IPlayerActionInput _input;
        private readonly IPlayerControl _control;
        private readonly ICommandDispatch _commands;
        private readonly CancellationTokenSource _lifetime =
            new();

        public PlayerActionController(
            IPlayerActionInput input,
            IPlayerControl control,
            ICommandDispatch commands)
        {
            _input = input;
            _control = control;
            _commands = commands;
        }

        public void Start()
        {
            _input.InteractPerformed += Interact;
            _input.PossessPerformed += Possess;
        }

        public void Dispose()
        {
            _input.InteractPerformed -= Interact;
            _input.PossessPerformed -= Possess;

            _lifetime.Cancel();
            _lifetime.Dispose();
        }

        private void Interact()
        {
            InteractAsync().Forget();
        }

        private void Possess()
        {
            PossessAsync().Forget();
        }

        private async UniTaskVoid InteractAsync()
        {
            var interactorInstanceId =
                _control.ControlledInstanceId;

            var target = _control.CurrentTarget;

            if (interactorInstanceId == Guid.Empty ||
                target == null ||
                target.InstanceId == Guid.Empty)
            {
                return;
            }

            var command = new InteractCommand(
                interactorInstanceId,
                _control.InteractionOrigin);

            await _commands.SendAsync(
                target.InstanceId,
                command,
                _lifetime.Token);
        }

        private async UniTaskVoid PossessAsync()
        {
            var target = _control.CurrentTarget;

            if (target == null ||
                target.InstanceId == Guid.Empty)
            {
                return;
            }

            await _control.PossessAsync(
                target.InstanceId,
                _lifetime.Token);
        }
    }
}