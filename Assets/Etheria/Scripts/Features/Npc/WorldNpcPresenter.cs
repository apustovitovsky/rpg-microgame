using System;
using System.Collections.Generic;
using Etheria.Game.Character;
using Etheria.Game.Npc;
using Etheria.Game.World;
using UnityEngine;
using VContainer.Unity;

namespace Etheria.Npc
{
    public sealed class WorldNpcPresenter :
        IStartable,
        IDisposable
    {
        private readonly ICharacterWorldStateService _worldState;
        private readonly INavigationLocationResolver _locations;
        private readonly INavigationGraphProvider _graphProvider;
        private readonly INpcStateRegistry _npcStates;
        private readonly INpcSpawner _spawner;

        private readonly Dictionary<string, GameObject> _instances =
            new(StringComparer.Ordinal);

        public WorldNpcPresenter(
            ICharacterWorldStateService worldState,
            INavigationLocationResolver locations,
            INavigationGraphProvider graphProvider,
            INpcStateRegistry npcStates,
            INpcSpawner spawner)
        {
            _worldState = worldState;
            _locations = locations;
            _graphProvider = graphProvider;
            _npcStates = npcStates;
            _spawner = spawner;
        }

        public void Start()
        {
            _worldState.CharacterChanged += OnCharacterChanged;

            foreach (var state in _worldState.States)
                Synchronize(state);
        }

        public void Dispose()
        {
            _worldState.CharacterChanged -= OnCharacterChanged;
            _instances.Clear();
        }

        private void OnCharacterChanged(string characterId)
        {
            if (_worldState.TryGetState(characterId, out var state))
                Synchronize(state);
            else
                Despawn(characterId);
        }

        private void Synchronize(WorldCharacterState state)
        {
            if (!TryResolveSpawnNode(state, out var node))
            {
                Despawn(state.CharacterId);
                return;
            }

            var npcState = _npcStates.GetOrCreate(state.CharacterId);
            npcState.AttachToNode(node.Id);
            npcState.SetLocation(state.LocationId);

            if (_instances.TryGetValue(
                    state.CharacterId,
                    out var instance) &&
                instance != null)
            {
                return;
            }

            _instances[state.CharacterId] =
                _spawner.Spawn(
                    state.CharacterId,
                    node.Position,
                    node.Rotation);
        }

        private bool TryResolveSpawnNode(
            WorldCharacterState state,
            out NavigationNode node)
        {
            node = default;

            if (!state.IsAlive)
            {
                Debug.LogWarning(
                    $"NPC '{state.CharacterId}' was not spawned: character is not alive.");
                return false;
            }

            if (!state.IsPresent)
            {
                Debug.LogWarning(
                    $"NPC '{state.CharacterId}' was not spawned: character is not present.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(state.LocationId))
            {
                Debug.LogWarning(
                    $"NPC '{state.CharacterId}' was not spawned: location id is empty.");
                return false;
            }

            if (_graphProvider.Graph == null)
            {
                Debug.LogWarning(
                    $"NPC '{state.CharacterId}' was not spawned: navigation graph is missing.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(state.AnchorKey))
            {
                Debug.LogWarning(
                    $"NPC '{state.CharacterId}' was not spawned: anchor key is empty.");
                return false;
            }

            if (!_locations.TryResolveAnchorNodeId(
                    state.LocationId,
                    state.AnchorKey,
                    out var nodeId))
            {
                Debug.LogWarning(
                    $"NPC '{state.CharacterId}' was not spawned: navigation location '{state.LocationId}' has no anchor '{state.AnchorKey}'.");
                return false;
            }

            if (!_graphProvider.Graph.TryGetNode(
                    nodeId,
                    out node))
            {
                Debug.LogWarning(
                    $"NPC '{state.CharacterId}' was not spawned: navigation node '{nodeId}' was not found in graph.");
                return false;
            }

            return true;
        }

        private void Despawn(string characterId)
        {
            if (!_instances.Remove(characterId, out var instance) ||
                instance == null)
            {
                return;
            }

            _npcStates.GetOrCreate(characterId).Detach();

            UnityEngine.Object.Destroy(instance);
        }
    }
}