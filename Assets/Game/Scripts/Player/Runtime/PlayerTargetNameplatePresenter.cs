using System;
using Game.Actor;
using Game.Pickup;
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
        private readonly IWorldRegistry<ITargetProvider> _targetProviders;
        private readonly IWorldRegistry<IWorldActor> _actors;
        private readonly IWorldRegistry<IWorldPickup> _pickups;

        private ITargetProvider _targetProvider;
        private ActorNameplateView _currentView;
        private ITargetable _currentTarget;
        private Camera _camera;

        public PlayerTargetNameplatePresenter(
            IPlayerService player,
            ActorNameplatePool pool,
            IWorldRegistry<ITargetProvider> targetProviders,
            IWorldRegistry<IWorldActor> actors,
            IWorldRegistry<IWorldPickup> pickups)
        {
            _player = player;
            _pool = pool;
            _targetProviders = targetProviders;
            _actors = actors;
            _pickups = pickups;
        }

        public void Start()
        {
            _player.CurrentActorChanged += OnCurrentActorChanged;
            OnCurrentActorChanged();
        }

        public void Dispose()
        {
            _player.CurrentActorChanged -= OnCurrentActorChanged;
            UnbindTargetProvider();
            ReleaseCurrent();
        }

        private void OnCurrentActorChanged()
        {
            UnbindTargetProvider();
            ReleaseCurrent();

            var actor = _player.CurrentActor;

            if (actor == null)
                return;

            if (!_targetProviders.TryGet(actor.WorldId, out _targetProvider))
                return;

            _targetProvider.CurrentTargetChanged += OnCurrentTargetChanged;
            OnCurrentTargetChanged(_targetProvider.CurrentTarget);
        }

        private void UnbindTargetProvider()
        {
            if (_targetProvider != null)
                _targetProvider.CurrentTargetChanged -= OnCurrentTargetChanged;

            _targetProvider = null;
        }

        private void OnCurrentTargetChanged(ITargetable target)
        {
            if (target == null ||
                !target.IsTargetable ||
                target.TargetPoint == null ||
                target.WorldId.IsEmpty)
            {
                ReleaseCurrent();
                return;
            }

            var currentActor = _player.CurrentActor;

            if (currentActor != null &&
                target.WorldId == currentActor.WorldId)
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
                target.TargetPoint,
                ResolveTargetName(target.WorldId),
                camera);
        }

        private string ResolveTargetName(WorldId worldId)
        {
            if (_actors.TryGet(worldId, out var actor) &&
                actor.Definition != null &&
                !string.IsNullOrWhiteSpace(actor.Definition.DisplayName))
            {
                return actor.Definition.DisplayName;
            }

            if (_pickups.TryGet(worldId, out var pickup) &&
                pickup.Definition != null &&
                !string.IsNullOrWhiteSpace(pickup.Definition.DisplayName))
            {
                return pickup.Definition.DisplayName;
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