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

        public WorldId CurrentActor { get; private set; }

        public event Action CurrentActorChanged;

        public void BindActor(WorldId actorWorldId)
        {
            if (actorWorldId.IsEmpty ||
                CurrentActor == actorWorldId)
            {
                return;
            }

            if (!_actors.TryGet(actorWorldId, out var actor) ||
                actor.InputBinder == null)
            {
                Debug.LogWarning(
                    $"Player cannot bind actor '{actorWorldId}': input binder is missing.");

                return;
            }

            if (!CurrentActor.IsEmpty &&
                _actors.TryGet(CurrentActor, out var currentActor) &&
                currentActor.InputBinder != null)
            {
                currentActor.InputBinder.Unbind();
            }

            CurrentActor = actorWorldId;
            actor.InputBinder.Bind(_input);

            BindCamera(actor);
            CurrentActorChanged?.Invoke();
        }

        public void UnbindActor(WorldId actorWorldId)
        {
            if (CurrentActor != actorWorldId)
                return;

            if (_actors.TryGet(CurrentActor, out var actor) &&
                actor.InputBinder != null)
            {
                actor.InputBinder.Unbind();
            }

            CurrentActor = default;

            BindCamera(null);
            CurrentActorChanged?.Invoke();
        }

        private void BindCamera(IWorldActor actor)
        {
            if (_camera == null)
            {
                Debug.LogWarning("Player camera is null.");
                return;
            }

            if (actor?.Transform != null)
            {
                _camera.Follow = actor.Transform.CameraPivot;
            }
            else
            {
                _camera.Follow = null;
            }

            _camera.LookAt = null;
        }
    }
}