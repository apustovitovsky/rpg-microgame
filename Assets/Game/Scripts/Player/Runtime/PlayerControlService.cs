using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Commands;
using Game.Control;
using Game.Input;
using Game.Interaction;
using Game.Targeting;
using Unity.Cinemachine;
using UnityEngine;

namespace Game.Player
{
    public sealed class PlayerControlService :
        IPlayerControl
    {
        private readonly ICommandDispatch _commands;
        private readonly CinemachineCamera _camera;
        private readonly IControlInput _input;

        private IPossessionEndpoint _endpoint;
        private IInteractor _interactor;
        private ITargetProvider _targetProvider;

        public PlayerControlService(
            ICommandDispatch commands,
            CinemachineCamera camera,
            IControlInput input)
        {
            _commands = commands;
            _camera = camera;
            _input = input;
        }

        public Guid ControlledInstanceId { get; private set; }

        public Vector3 ControlledPosition =>
            _endpoint != null
                ? _endpoint.Root.position
                : Vector3.zero;

        public Vector3 InteractionOrigin =>
            _interactor != null
                ? _interactor.InteractionOrigin
                : Vector3.zero;

        public ITargetable CurrentTarget =>
            _targetProvider?.CurrentTarget;

        public event Action ControlledObjectChanged;

        public event Action CurrentTargetChanged;

        public UniTask<CommandResult> PossessAsync(
            Guid targetInstanceId,
            CancellationToken token)
        {
            if (targetInstanceId == Guid.Empty)
            {
                return UniTask.FromResult(
                    CommandResult.Rejected);
            }

            if (targetInstanceId == ControlledInstanceId)
            {
                return UniTask.FromResult(
                    CommandResult.Completed);
            }

            return _commands.SendAsync(
                targetInstanceId,
                new PossessCommand(),
                token);
        }

        public void Release()
        {
            if (ControlledInstanceId == Guid.Empty)
                return;

            ReleaseCurrent();

            ControlledObjectChanged?.Invoke();
            CurrentTargetChanged?.Invoke();
        }

        public CommandResult Attach(
            Guid instanceId,
            IPossessionEndpoint endpoint,
            IInteractor interactor,
            ITargetProvider targetProvider)
        {
            if (instanceId == Guid.Empty ||
                endpoint == null ||
                interactor == null ||
                targetProvider == null)
            {
                return CommandResult.Rejected;
            }

            if (instanceId == ControlledInstanceId)
                return CommandResult.Completed;

            ReleaseCurrent();

            ControlledInstanceId = instanceId;
            _endpoint = endpoint;
            _interactor = interactor;
            _targetProvider = targetProvider;

            _endpoint.BindInput(_input);
            _targetProvider.CurrentTargetChanged +=
                OnCurrentTargetChanged;

            BindCamera(_endpoint);

            ControlledObjectChanged?.Invoke();
            CurrentTargetChanged?.Invoke();

            return CommandResult.Completed;
        }

        public void ReleaseIfCurrent(
            IPossessionEndpoint endpoint)
        {
            if (endpoint == null ||
                !ReferenceEquals(_endpoint, endpoint))
            {
                return;
            }

            Release();
        }

        private void ReleaseCurrent()
        {
            if (_targetProvider != null)
            {
                _targetProvider.CurrentTargetChanged -=
                    OnCurrentTargetChanged;
            }

            _endpoint?.UnbindInput();

            ControlledInstanceId = Guid.Empty;
            _endpoint = null;
            _interactor = null;
            _targetProvider = null;

            BindCamera(null);
        }

        private void OnCurrentTargetChanged(
            ITargetable _)
        {
            CurrentTargetChanged?.Invoke();
        }

        private void BindCamera(
            IPossessionEndpoint endpoint)
        {
            if (_camera == null)
            {
                Debug.LogWarning("Player camera is null.");
                return;
            }

            _camera.Follow = endpoint?.CameraPivot;
            _camera.LookAt = null;
        }
    }
}