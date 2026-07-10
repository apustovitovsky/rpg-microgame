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
        private readonly IActorService _actors;

        public PlayerService(
            CinemachineCamera camera,
            IActorInput input,
            IActorService actors)
        {
            _camera = camera;
            _input = input;
            _actors = actors;
        }

        public Guid CurrentActor { get; private set; }

        public event Action CurrentActorChanged;

        public void BindActor(Guid actorInstanceId)
        {
            if (actorInstanceId == Guid.Empty ||
                CurrentActor == actorInstanceId)
            {
                return;
            }

            if (!_actors.TryGet(
                    actorInstanceId,
                    out var actor) ||
                actor.InputBinder == null)
            {
                Debug.LogWarning(
                    $"Player cannot bind actor '{actorInstanceId:N}': " +
                    "input binder is missing.");

                return;
            }

            if (CurrentActor != Guid.Empty &&
                _actors.TryGet(
                    CurrentActor,
                    out var currentActor) &&
                currentActor.InputBinder != null)
            {
                currentActor.InputBinder.Unbind();
            }

            CurrentActor = actorInstanceId;
            actor.InputBinder.Bind(_input);

            BindCamera(actor);
            CurrentActorChanged?.Invoke();
        }

        public void UnbindActor(Guid actorInstanceId)
        {
            if (CurrentActor != actorInstanceId)
                return;

            if (_actors.TryGet(
                    CurrentActor,
                    out var actor) &&
                actor.InputBinder != null)
            {
                actor.InputBinder.Unbind();
            }

            CurrentActor = Guid.Empty;

            BindCamera(null);
            CurrentActorChanged?.Invoke();
        }

        private void BindCamera(IActorRuntime actor)
        {
            if (_camera == null)
            {
                Debug.LogWarning("Player camera is null.");
                return;
            }

            _camera.Follow = actor?.View != null
                ? actor.View.CameraPivot
                : null;

            _camera.LookAt = null;
        }
    }
}