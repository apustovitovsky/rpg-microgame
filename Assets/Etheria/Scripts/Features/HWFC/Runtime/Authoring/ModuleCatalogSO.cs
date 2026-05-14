using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Etheria.Features.HWFC {

[CreateAssetMenu(menuName = "Etheria/HWFC/Module Catalog", fileName = "ModuleCatalog")]
public class ModuleCatalogSO : ScriptableObject {
	public ModuleDefinitionSO[] Items;

	public IEnumerable<ModuleDefinitionSO> GetValidItems() {
		return (Items ?? new ModuleDefinitionSO[0]).Where(item => item != null);
	}
}
}
