using System;
using System.Collections.Generic;

namespace Etheria.Features.StoryletSystem
{
    public sealed class StoryletSmokeRegistryException : InvalidOperationException
    {
        public StoryletSmokeRegistryException(string message) : base(message)
        {
        }
    }

    internal sealed class OrderedSymbolRegistry<TSymbol>
        where TSymbol : struct, IStoryletSymbol
    {
        private readonly Dictionary<string, byte> _indexByKey;
        private readonly string _typeName;

        public OrderedSymbolRegistry(string typeName, IReadOnlyList<TSymbol> symbols)
        {
            _typeName = typeName ?? throw new ArgumentNullException(nameof(typeName));
            symbols = symbols ?? throw new ArgumentNullException(nameof(symbols));

            if (symbols.Count > TagSet.Capacity)
            {
                throw new StoryletSmokeRegistryException(
                    $"{_typeName} registry exceeds {TagSet.Capacity} symbols.");
            }

            _indexByKey = new Dictionary<string, byte>(symbols.Count, StringComparer.Ordinal);

            for (var i = 0; i < symbols.Count; i++)
            {
                var key = symbols[i].Key;

                if (string.IsNullOrWhiteSpace(key))
                {
                    throw new StoryletSmokeRegistryException($"{_typeName} registry contains an empty key.");
                }

                if (_indexByKey.ContainsKey(key))
                {
                    throw new StoryletSmokeRegistryException(
                        $"{_typeName} registry contains duplicate key '{key}'.");
                }

                _indexByKey[key] = checked((byte)i);
            }
        }

        public int Resolve(TSymbol symbol)
        {
            if (_indexByKey.TryGetValue(symbol.Key, out var index))
            {
                return index;
            }

            throw new StoryletSmokeRegistryException(
                $"{_typeName} symbol '{symbol.Key}' is not registered in the canonical symbol list.");
        }
    }
}
