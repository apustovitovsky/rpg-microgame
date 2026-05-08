using System.Collections.Generic;
using UnityEngine;


namespace Etheria.Features.HWFC
{
    [CreateAssetMenu(
        menuName = "Etheria/HWFC/Tile Catalog",
        fileName = "TileCatalog")]
    public sealed class TileCatalogSO : ScriptableObject
    {
        [SerializeField] private TileDefinitionSO[] _items = new TileDefinitionSO[0];

        public IReadOnlyList<TileDefinitionSO> Items => _items;
    }
}
