using System;
using System.Collections.Generic;
using Game.Actor;
using Game.Navigation;
using UnityEngine;

namespace Game.Gameplay
{
    [CreateAssetMenu(
        fileName = "EntitySpawnCatalog",
        menuName = "Game/Gameplay/Entity Spawn Catalog")]
    public sealed class ActorSpawnCatalog :
        ScriptableObject
    {
        [SerializeField] private ActorEntry _player = new();

        [SerializeField]
        private ActorEntry[] _actors =
            Array.Empty<ActorEntry>();

        public ActorEntry Player => _player;

        public IReadOnlyList<ActorEntry> Actors => _actors;

        private void OnValidate()
        {
            _player?.Normalize();

            if (_actors == null)
                return;

            foreach (var actor in _actors)
                actor?.Normalize();
        }

        [Serializable]
        public sealed class ActorEntry
        {
            [SerializeField] private string _definitionId;
            [SerializeField] private string _locationId;

            [SerializeField]
            private string _anchorKey =
                NavigationAnchorKeys.Default;

            [SerializeField]
            private PatrolPoint[] _patrolPoints =
                Array.Empty<PatrolPoint>();

            public string DefinitionId => _definitionId;

            public string LocationId => _locationId;

            public string AnchorKey => _anchorKey;

            public IReadOnlyList<PatrolPoint> PatrolPoints =>
                _patrolPoints;

            public ActorRuntime CreateRuntime(Guid instanceId)
            {
                return new ActorRuntime(
                    instanceId,
                    new ActorLocation(
                        _locationId,
                        _anchorKey),
                    CreatePatrolLocations());
            }

            public void Normalize()
            {
                _definitionId = _definitionId?.Trim();
                _locationId = _locationId?.Trim();

                _anchorKey = string.IsNullOrWhiteSpace(_anchorKey)
                    ? NavigationAnchorKeys.Default
                    : _anchorKey.Trim();

                if (_patrolPoints == null)
                    return;

                foreach (var point in _patrolPoints)
                    point?.Normalize();
            }

            private ActorLocation[] CreatePatrolLocations()
            {
                if (_patrolPoints == null ||
                    _patrolPoints.Length == 0)
                {
                    return Array.Empty<ActorLocation>();
                }

                var locations =
                    new ActorLocation[_patrolPoints.Length];

                for (var index = 0;
                     index < _patrolPoints.Length;
                     index++)
                {
                    var point = _patrolPoints[index];

                    if (point == null || !point.IsValid)
                    {
                        throw new InvalidOperationException(
                            $"Patrol point {index} for actor " +
                            $"'{_definitionId}' is invalid.");
                    }

                    locations[index] = new ActorLocation(
                        point.LocationId,
                        point.AnchorKey);
                }

                return locations;
            }
        }

        [Serializable]
        public sealed class PatrolPoint
        {
            [SerializeField] private string _locationId;

            [SerializeField]
            private string _anchorKey =
                NavigationAnchorKeys.Default;

            public string LocationId => _locationId;

            public string AnchorKey => _anchorKey;

            public bool IsValid =>
                !string.IsNullOrWhiteSpace(_locationId) &&
                !string.IsNullOrWhiteSpace(_anchorKey);

            public void Normalize()
            {
                _locationId = _locationId?.Trim();

                _anchorKey = string.IsNullOrWhiteSpace(_anchorKey)
                    ? NavigationAnchorKeys.Default
                    : _anchorKey.Trim();
            }
        }
    }
}