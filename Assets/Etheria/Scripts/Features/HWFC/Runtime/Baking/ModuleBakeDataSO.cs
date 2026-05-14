using UnityEngine;

namespace Etheria.Features.HWFC {

[CreateAssetMenu(menuName = "Etheria/HWFC/Module Bake Data", fileName = "ModuleBakeData")]
public class ModuleBakeDataSO : ScriptableObject {
	public ModuleCatalogSO SourceCatalog;
	public BakedModuleVariant[] Variants;
	public BakedAdjacencyData Adjacency;
	public bool Simplified;
	public string SourceFingerprint;

	public bool IsValid() {
		return Variants != null
			&& Adjacency != null
			&& Adjacency.AllowedNeighborIndices != null
			&& Adjacency.ModuleCount == Variants.Length
			&& Adjacency.AllowedNeighborIndices.Length == Variants.Length * 6;
	}

	public int ModuleCount => Variants != null ? Variants.Length : 0;
}
}
