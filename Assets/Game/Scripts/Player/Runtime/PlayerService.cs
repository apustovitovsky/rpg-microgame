using System;
using Game.Actor;
using Game.Core;
using Game.Input;
using Unity.Cinemachine;
using UnityEngine;

namespace Game.Player
{
    public sealed class PlayerService : IPlayerService
    {
        private readonly CinemachineCamera _camera;
        private readonly IActorInput _input;
        private readonly IInstanceRegistry<IPossessable> _possessables;

        public PlayerService(
            CinemachineCamera camera,
            IActorInput input,
            IInstanceRegistry<IPossessable> possessables)
        {
            _camera = camera;
            _input = input;
            _possessables = possessables;
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

            if (!_possessables.TryGet(
                    actorInstanceId,
                    out var possessable))
            {
                Debug.LogWarning(
                    $"Player cannot bind actor '{actorInstanceId:N}': " +
                    "possessable capability is missing.");

                return;
            }

            UnbindCurrent();

            CurrentActor = actorInstanceId;
            possessable.BindInput(_input);

            BindCamera(possessable);
            CurrentActorChanged?.Invoke();
        }

        public void UnbindActor(Guid actorInstanceId)
        {
            if (CurrentActor != actorInstanceId)
                return;

            UnbindCurrent();

            CurrentActor = Guid.Empty;

            BindCamera(null);
            CurrentActorChanged?.Invoke();
        }

        private void UnbindCurrent()
        {
            if (CurrentActor == Guid.Empty)
                return;

            if (_possessables.TryGet(
                    CurrentActor,
                    out var currentPossessable))
            {
                currentPossessable.UnbindInput();
            }
        }

        private void BindCamera(IPossessable possessable)
        {
            if (_camera == null)
            {
                Debug.LogWarning("Player camera is null.");
                return;
            }

            _camera.Follow = possessable?.CameraPivot;
            _camera.LookAt = null;
        }
    }
}