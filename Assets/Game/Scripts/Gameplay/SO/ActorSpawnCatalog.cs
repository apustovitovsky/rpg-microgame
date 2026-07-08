using System;
using System.Collections.Generic;
using Etheria.Game.World;
using Game.Actor;
using UnityEngine;

namespace Game.Gameplay
{
    [CreateAssetMenu(
        fileName = "ActorSpawnCatalog",
        menuName = "Game/Gameplay/Actor Spawn Catalog")]
    public sealed class ActorSpawnCatalog : ScriptableObject
    {
        [SerializeField] private ActorEntry _player = new();
        [SerializeField] private ActorEntry[] _actors = Array.Empty<ActorEntry>();

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
            [SerializeField] private ActorDefinition _definition;
            [SerializeField] private string _locationId;
            [SerializeField] private string _anchorKey = NavigationAnchorKeys.Default;

            public ActorDefinition Definition => _definition;
            public string LocationId => _locationId;
            public string AnchorKey => _anchorKey;

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