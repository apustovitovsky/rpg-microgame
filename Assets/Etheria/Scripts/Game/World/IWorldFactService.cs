using System;
using System.Collections.Generic;

namespace Etheria.Game.World
{
    public interface IWorldFactService
    {
        IReadOnlyCollection<string> ActiveFacts { get; }

        bool IsSet(string factId);
        bool TrySet(string factId);
        bool TryClear(string factId);

        event Action<string, bool> FactChanged;
    }
}