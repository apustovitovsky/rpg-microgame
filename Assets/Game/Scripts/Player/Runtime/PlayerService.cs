using System;
using Game.Actor;
using Unity.Cinemachine;
using UnityEngine;

namespace Game.Player
{
    public sealed class PlayerService : IPlayerService
    {
        private readonly CinemachineCamera _camera;

        public PlayerService(CinemachineCamera camera)
        {
            _camera = camera;
        }

        public ActorInstance CurrentActor { get; private set; }

        public event Action CurrentActorChanged;

        public void BindActor(ActorInstance actor)
        {
            if (ReferenceEquals(CurrentActor, actor))
                return;

            CurrentActor = actor;
            BindCamera(actor);
            CurrentActorChanged?.Invoke();
        }

        public void UnbindActor(ActorInstance actor)
        {
            if (!ReferenceEquals(CurrentActor, actor))
                return;

            CurrentActor = null;
            BindCamera(null);
            CurrentActorChanged?.Invoke();
        }

        private void BindCamera(ActorInstance actor)
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