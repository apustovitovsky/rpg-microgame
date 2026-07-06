using System;
using Game.World;

namespace Game.Actor
{
    public sealed class ActorIdentity :
        IActorIdentity
    {
        private WorldId _worldId;
        private string _displayName = string.Empty;

        public WorldId WorldId => _worldId;
        public string DisplayName => _displayName;

        public void Initialize(
            WorldId worldId,
            string displayName)
        {
            displayName = displayName?.Trim() ?? string.Empty;

            if (worldId.IsEmpty)
                throw new ArgumentException("Actor world id is required.", nameof(worldId));

            _worldId = worldId;
            _displayName = string.IsNullOrWhiteSpace(displayName)
                ? worldId.ToString()
                : displayName;
        }
    }
}