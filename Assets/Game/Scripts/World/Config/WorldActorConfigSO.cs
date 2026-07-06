using System;
using System.Collections.Generic;
using Etheria.Game.World;
using UnityEngine;

namespace Game.World
{
    [CreateAssetMenu(
        fileName = "WorldActorConfig",
        menuName = "Game/World/World Actor Config")]
    public sealed class WorldActorConfigSO : ScriptableObject
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
            [SerializeField] private string _definitionId;
            [SerializeField] private GameObject _prefab;
            [SerializeField] private string _locationId;
            [SerializeField] private string _anchorKey = NavigationAnchorKeys.Default;

            public string DefinitionId => _definitionId;
            public GameObject Prefab => _prefab;
            public string LocationId => _locationId;
            public string AnchorKey => _anchorKey;

            public void Normalize()
            {
                _definitionId = _definitionId?.Trim();
                _locationId = _locationId?.Trim();

                _anchorKey = string.IsNullOrWhiteSpace(_anchorKey)
                    ? NavigationAnchorKeys.Default
                    : _anchorKey.Trim();
            }
        }
    }
}