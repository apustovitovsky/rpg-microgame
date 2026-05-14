using System.Collections.Generic;
using UnityEngine;


namespace Etheria.Features.HWFC_Old
{
    [CreateAssetMenu(
        menuName = "Etheria/HWFC_Old/Module Catalog",
        fileName = "ModuleCatalog")]
    public sealed class ModuleCatalogSO : ScriptableObject
    {
        [SerializeField] private ModuleDataSO[] _items = new ModuleDataSO[0];

        public IReadOnlyList<ModuleDataSO> Items => _items;
    }
}
