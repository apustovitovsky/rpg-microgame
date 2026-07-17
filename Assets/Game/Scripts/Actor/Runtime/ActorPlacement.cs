using System;
using System.Collections.Generic;

namespace Game.Actor
{
    public sealed class ActorPlacement
    {
        private readonly ActorLocation[] _patrolLocations;

        public ActorPlacement(
            ActorLocation spawnLocation,
            IReadOnlyList<ActorLocation> patrolLocations)
        {
            if (!spawnLocation.IsValid)
            {
                throw new ArgumentException(
                    "Actor spawn location is invalid.",
                    nameof(spawnLocation));
            }

            if (patrolLocations == null)
            {
                throw new ArgumentNullException(
                    nameof(patrolLocations));
            }

            _patrolLocations =
                new ActorLocation[patrolLocations.Count];

            for (var index = 0;
                 index < patrolLocations.Count;
                 index++)
            {
                var location = patrolLocations[index];

                if (!location.IsValid)
                {
                    throw new ArgumentException(
                        $"Patrol location {index} is invalid.",
                        nameof(patrolLocations));
                }

                _patrolLocations[index] = location;
            }

            SpawnLocation = spawnLocation;
        }

        public ActorLocation SpawnLocation { get; }

        public IReadOnlyList<ActorLocation> PatrolLocations =>
            _patrolLocations;

        public bool HasPatrol =>
            _patrolLocations.Length > 0;
    }

    public readonly struct ActorLocation
    {
        public ActorLocation(
            string locationId,
            string anchorKey)
        {
            LocationId = locationId;
            AnchorKey = anchorKey;
        }

        public string LocationId { get; }

        public string AnchorKey { get; }

        public bool IsValid =>
            !string.IsNullOrWhiteSpace(LocationId) &&
            !string.IsNullOrWhiteSpace(AnchorKey);
    }
}