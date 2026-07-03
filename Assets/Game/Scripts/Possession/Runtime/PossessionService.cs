using System;
using Game.Actor;
using Unity.Cinemachine;

namespace Game.Possession
{
    public sealed class PossessionService : IPossessionService
    {
        private readonly CinemachineCamera _camera;

        public PossessionService(CinemachineCamera camera)
        {
            _camera = camera;
        }

        public IActorView CurrentActor { get; private set; }

        public event Action CurrentActorChanged;

        public void Possess(IActorView actor)
        {
            if (ReferenceEquals(CurrentActor, actor))
                return;

            CurrentActor = actor;
            BindCamera(actor);
            CurrentActorChanged?.Invoke();
        }

        public void Unpossess(IActorView actor)
        {
            if (!ReferenceEquals(CurrentActor, actor))
                return;

            CurrentActor = null;
            BindCamera(null);
            CurrentActorChanged?.Invoke();
        }

        private void BindCamera(IActorView actor)
        {
            if (_camera == null)
                return;

            _camera.Follow = actor?.CameraPivot;
            _camera.LookAt = null;
        }
    }
}