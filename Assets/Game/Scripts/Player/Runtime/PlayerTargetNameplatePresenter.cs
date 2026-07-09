using System;
using Game.Actor;
using Game.Targeting;
using Game.World;
using UnityEngine;
using VContainer.Unity;

namespace Game.Player
{
    public sealed class PlayerTargetNameplatePresenter :
        IStartable,
        IDisposable
    {
        private readonly IPlayerService _player;
        private readonly ActorNameplatePool _pool;
        private readonly IActorService _actors;
        private readonly IWorldObjectRegistry _world;

        private IActorTargeting _targeting;
        private ActorNameplateView _currentView;
        private ITargetable _currentTarget;
        private Camera _camera;

        public PlayerTargetNameplatePresenter(
            IPlayerService player,
            ActorNameplatePool pool,
            IActorService actors,
            IWorldObjectRegistry world)
        {
            _player = player;
            _pool = pool;
            _actors = actors;
            _world = world;
        }

        public void Start()
        {
            _player.CurrentActorChanged += OnCurrentActorChanged;
            OnCurrentActorChanged();
        }

        public void Dispose()
        {
            _player.CurrentActorChanged -= OnCurrentActorChanged;
            UnbindTargeting();
            ReleaseCurrent();
        }

        private void OnCurrentActorChanged()
        {
            UnbindTargeting();
            ReleaseCurrent();

            var actorWorldId = _player.CurrentActor;

            if (actorWorldId.IsEmpty)
                return;

            if (!_actors.TryGet(actorWorldId, out var actor) ||
                actor.Targeting == null)
            {
                return;
            }

            _targeting = actor.Targeting;
            _targeting.CurrentTargetChanged += OnCurrentTargetChanged;
            OnCurrentTargetChanged(_targeting.CurrentTarget);
        }

        private void UnbindTargeting()
        {
            if (_targeting != null)
                _targeting.CurrentTargetChanged -= OnCurrentTargetChanged;

            _targeting = null;
        }

        private void OnCurrentTargetChanged(ITargetable target)
        {
            if (target == null ||
                !target.IsTargetable ||
                target.WorldId.IsEmpty)
            {
                ReleaseCurrent();
                return;
            }

            var currentActor = _player.CurrentActor;

            if (!currentActor.IsEmpty &&
                target.WorldId == currentActor)
            {
                ReleaseCurrent();
                return;
            }

            if (ReferenceEquals(_currentTarget, target))
                return;

            var camera = ResolveCamera();

            if (camera == null)
            {
                ReleaseCurrent();
                return;
            }

            ReleaseCurrent();

            _currentTarget = target;
            _currentView = _pool.Get(
                target.UiAnchor,
                ResolveTargetName(target.WorldId),
                camera);
        }

        private string ResolveTargetName(WorldId worldId)
        {
            if (_world.TryGetInfo(worldId, out var info) &&
                !string.IsNullOrWhiteSpace(info.DisplayName))
            {
                return info.DisplayName;
            }

            return worldId.ToString();
        }

        private Camera ResolveCamera()
        {
            if (_camera != null)
                return _camera;

            _camera = Camera.main;
            return _camera;
        }

        private void ReleaseCurrent()
        {
            if (_currentView != null)
                _pool.Release(_currentView);

            _currentView = null;
            _currentTarget = null;
        }
    }
}