using System;
using Game.Actor;
using Game.Input;
using Game.World;
using Unity.Cinemachine;
using UnityEngine;

namespace Game.Player
{
    public sealed class PlayerService : IPlayerService
    {
        private readonly CinemachineCamera _camera;
        private readonly IActorInput _input;
        private readonly IWorldRegistry<IActorInputBinder> _inputBinders;
        private readonly IWorldRegistry<IActorAnchors> _anchors;

        public PlayerService(
            CinemachineCamera camera,
            IActorInput input,
            IWorldRegistry<IActorInputBinder> inputBinders,
            IWorldRegistry<IActorAnchors> anchors)
        {
            _camera = camera;
            _input = input;
            _inputBinders = inputBinders;
            _anchors = anchors;
        }

        public IWorldHandle CurrentActor { get; private set; }

        public event Action CurrentActorChanged;

        public void BindActor(IWorldHandle actor)
        {
            if (actor == null ||
                ReferenceEquals(CurrentActor, actor))
            {
                return;
            }

            if (!_inputBinders.TryGet(actor.WorldId, out var inputBinder))
            {
                Debug.LogWarning(
                    $"Player cannot bind actor '{actor.WorldId}': input binder is missing.");

                return;
            }

            if (CurrentActor != null &&
                _inputBinders.TryGet(CurrentActor.WorldId, out var currentInputBinder))
            {
                currentInputBinder.Unbind();
            }

            CurrentActor = actor;
            inputBinder.Bind(_input);

            BindCamera(actor);
            CurrentActorChanged?.Invoke();
        }

        public void UnbindActor(IWorldHandle actor)
        {
            if (!ReferenceEquals(CurrentActor, actor))
                return;

            if (_inputBinders.TryGet(CurrentActor.WorldId, out var inputBinder))
                inputBinder.Unbind();

            CurrentActor = null;

            BindCamera(null);
            CurrentActorChanged?.Invoke();
        }

        private void BindCamera(IWorldHandle actor)
        {
            if (_camera == null)
            {
                Debug.LogWarning("Player camera is null.");
                return;
            }

            if (actor != null &&
                _anchors.TryGet(actor.WorldId, out var anchors))
            {
                _camera.Follow = anchors.CameraPivot;
            }
            else
            {
                _camera.Follow = null;
            }

            _camera.LookAt = null;
        }
    }
}