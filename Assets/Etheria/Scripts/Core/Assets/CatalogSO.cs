using System.Collections.Generic;
using UnityEngine;

namespace Etheria.Core.Assets
{
    public abstract class CatalogSO<TDefinition> : ScriptableObject
        where TDefinition : ScriptableObject
    {
        [SerializeField] private TDefinition[] _items;

        public IReadOnlyList<TDefinition> Items => _items;
    }
}