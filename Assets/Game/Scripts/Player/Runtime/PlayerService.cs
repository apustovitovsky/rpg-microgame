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
        private readonly IWorldRegistry<IActorView> _views;

        public PlayerService(
            CinemachineCamera camera,
            IActorInput input,
            IWorldRegistry<IActorInputBinder> inputBinders,
            IWorldRegistry<IActorView> views)
        {
            _camera = camera;
            _input = input;
            _inputBinders = inputBinders;
            _views = views;
        }

        public WorldId CurrentActor { get; private set; }

        public event Action CurrentActorChanged;

        public void BindActor(WorldId actorWorldId)
        {
            if (actorWorldId.IsEmpty ||
                CurrentActor == actorWorldId)
            {
                return;
            }

            if (!_inputBinders.TryGet(actorWorldId, out var inputBinder))
            {
                Debug.LogWarning(
                    $"Player cannot bind actor '{actorWorldId}': input binder is missing.");

                return;
            }

            if (!CurrentActor.IsEmpty &&
                _inputBinders.TryGet(CurrentActor, out var currentInputBinder))
            {
                currentInputBinder.Unbind();
            }

            CurrentActor = actorWorldId;
            inputBinder.Bind(_input);

            BindCamera(actorWorldId);
            CurrentActorChanged?.Invoke();
        }

        public void UnbindActor(WorldId actorWorldId)
        {
            if (CurrentActor != actorWorldId)
                return;

            if (_inputBinders.TryGet(CurrentActor, out var inputBinder))
                inputBinder.Unbind();

            CurrentActor = default;

            BindCamera(default);
            CurrentActorChanged?.Invoke();
        }

        private void BindCamera(WorldId actorWorldId)
        {
            if (_camera == null)
            {
                Debug.LogWarning("Player camera is null.");
                return;
            }

            if (!actorWorldId.IsEmpty &&
                _views.TryGet(actorWorldId, out var anchors))
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