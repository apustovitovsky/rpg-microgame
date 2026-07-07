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

        public PlayerService(
            CinemachineCamera camera,
            IActorInput input)
        {
            _camera = camera;
            _input = input;
        }

        public IWorldObject CurrentActor { get; private set; }

        public event Action CurrentActorChanged;

        public void BindActor(IWorldObject actor)
        {
            if (actor == null ||
                ReferenceEquals(CurrentActor, actor))
            {
                return;
            }

            if (!actor.TryGet<IActorInputBinder>(out var inputBinder))
            {
                Debug.LogWarning(
                    $"Player cannot bind actor '{actor.WorldId}': input binder is missing.");

                return;
            }

            if (CurrentActor != null &&
                CurrentActor.TryGet<IActorInputBinder>(out var currentInputBinder))
            {
                currentInputBinder.Unbind();
            }

            CurrentActor = actor;
            inputBinder.Bind(_input);

            BindCamera(actor);
            CurrentActorChanged?.Invoke();
        }

        public void UnbindActor(IWorldObject actor)
        {
            if (!ReferenceEquals(CurrentActor, actor))
                return;

            if (CurrentActor.TryGet<IActorInputBinder>(out var inputBinder))
                inputBinder.Unbind();

            CurrentActor = null;

            BindCamera(null);
            CurrentActorChanged?.Invoke();
        }

        private void BindCamera(IWorldObject actor)
        {
            if (_camera == null)
            {
                Debug.LogWarning("Player camera is null.");
                return;
            }

            if (actor != null &&
                actor.TryGet<IActorView>(out var view))
            {
                _camera.Follow = view.CameraPivot;
            }
            else
            {
                _camera.Follow = null;
            }

            _camera.LookAt = null;
        }
    }
}