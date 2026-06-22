using System;
using System.Collections.Generic;
using Etheria.Game.World;

namespace Etheria.Features.Campaign
{
    public sealed class WorldFactService : IWorldFactService
    {
        private readonly HashSet<string> _activeFacts =
            new(StringComparer.Ordinal);

        public IReadOnlyCollection<string> ActiveFacts =>
            _activeFacts;

        public event Action<string, bool> FactChanged;

        public bool IsSet(string factId)
        {
            ValidateFactId(factId);
            return _activeFacts.Contains(factId);
        }

        public bool TrySet(string factId)
        {
            ValidateFactId(factId);

            if (!_activeFacts.Add(factId))
                return false;

            FactChanged?.Invoke(factId, true);
            return true;
        }

        public bool TryClear(string factId)
        {
            ValidateFactId(factId);

            if (!_activeFacts.Remove(factId))
                return false;

            FactChanged?.Invoke(factId, false);
            return true;
        }

        private static void ValidateFactId(string factId)
        {
            if (string.IsNullOrWhiteSpace(factId))
            {
                throw new ArgumentException(
                    "World fact ID cannot be empty.",
                    nameof(factId));
            }
        }
    }
}