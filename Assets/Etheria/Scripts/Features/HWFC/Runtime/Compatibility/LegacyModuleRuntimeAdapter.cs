using System;
using System.Linq;

namespace Etheria.Features.HWFC {

// Temporary bridge from new baked SO-based data to the legacy HWFC runtime model.
// Remove after Map/Slot/ModuleSet/Module stop depending on ModuleData.Current and legacy Module[].
public static class LegacyModuleRuntimeAdapter {
	public static Module[] CreateLegacyModules(ModuleBakeDataSO bakeData) {
		if (bakeData == null) {
			throw new ArgumentNullException(nameof(bakeData));
		}
		if (!bakeData.IsValid()) {
			throw new InvalidOperationException("Module bake data is missing variants or adjacency.");
		}

		var modules = bakeData.Variants.Select(CreateLegacyModule).ToArray();
		ModuleData.Current = modules;
		ApplyLegacyNeighbors(modules, bakeData);
		return modules;
	}

	public static Module CreateLegacyModule(BakedModuleVariant variant) {
		return new Module(variant);
	}

	public static void ApplyLegacyNeighbors(Module[] legacyModules, ModuleBakeDataSO bakeData) {
		for (int moduleIndex = 0; moduleIndex < legacyModules.Length; moduleIndex++) {
			var module = legacyModules[moduleIndex];
			module.PossibleNeighbors = new ModuleSet[6];
			module.PossibleNeighborsArray = new Module[6][];

			for (int direction = 0; direction < 6; direction++) {
				int adjacencyIndex = moduleIndex * 6 + direction;
				var neighborIndices = bakeData.Adjacency.AllowedNeighborIndices[adjacencyIndex] != null
					? bakeData.Adjacency.AllowedNeighborIndices[adjacencyIndex].Indices ?? Array.Empty<int>()
					: Array.Empty<int>();
				var neighbors = neighborIndices.Select(index => legacyModules[index]);
				module.PossibleNeighbors[direction] = new ModuleSet(neighbors);
				module.PossibleNeighborsArray[direction] = neighborIndices.Select(index => legacyModules[index]).ToArray();
			}
		}
	}
}
}
