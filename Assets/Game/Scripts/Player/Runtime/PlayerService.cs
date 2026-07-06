using System;
using Game.Actor;
using Game.Input;
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

        public WorldActor CurrentActor { get; private set; }

        public event Action CurrentActorChanged;

        public void BindActor(WorldActor actor)
        {
            if (actor == null ||
                ReferenceEquals(CurrentActor, actor))
            {
                return;
            }

            if (actor.InputBinder == null)
            {
                Debug.LogWarning(
                    $"Player cannot bind actor '{actor.WorldId}': input binder is missing.");

                return;
            }

            CurrentActor?.InputBinder?.Unbind();

            CurrentActor = actor;
            CurrentActor.InputBinder.Bind(_input);

            BindCamera(actor);
            CurrentActorChanged?.Invoke();
        }

        public void UnbindActor(WorldActor actor)
        {
            if (!ReferenceEquals(CurrentActor, actor))
                return;

            CurrentActor.InputBinder?.Unbind();
            CurrentActor = null;

            BindCamera(null);
            CurrentActorChanged?.Invoke();
        }

        private void BindCamera(WorldActor actor)
        {
            if (_camera == null)
            {
                Debug.LogWarning("Player camera is null.");
                return;
            }

            _camera.Follow = actor?.View.CameraPivot;
            _camera.LookAt = null;
        }
    }
}