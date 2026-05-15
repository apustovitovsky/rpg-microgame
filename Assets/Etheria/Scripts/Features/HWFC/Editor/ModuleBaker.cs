using System;
using System.Collections.Generic;
using System.Linq;
using Etheria.Features.HWFC;
using UnityEditor;
using UnityEngine;

public static class ModuleBaker {
	public static ModuleBakeDataSO Bake(ModuleCatalogSO catalog, ModuleBakeDataSO target = null, bool simplify = false) {
		var errors = ModuleValidation.ValidateCatalog(catalog);
		if (errors.Any()) {
			throw new InvalidOperationException(string.Join("\n", errors.ToArray()));
		}

		if (target == null) {
			target = GetOrCreateBakeAsset(catalog);
		}

		var variants = BuildVariants(catalog);
		target.SourceCatalog = catalog;
		target.Variants = variants.ToArray();
		target.Adjacency = BuildAdjacency(variants);
		target.Simplified = false;
		target.SourceFingerprint = ComputeFingerprint(catalog);

		if (simplify) {
			SimplifyAdjacency(target);
			target.Simplified = true;
		}

		EditorUtility.SetDirty(target);
		AssetDatabase.SaveAssets();
		return target;
	}

	public static List<BakedModuleVariant> BuildVariants(ModuleCatalogSO catalog) {
		var variants = new List<BakedModuleVariant>();
		int index = 0;

		foreach (var definition in catalog.GetValidItems()) {
			int maxRotation = definition.AllowRotations ? 4 : 1;
			for (int rotation = 0; rotation < maxRotation; rotation++) {
				if (rotation != 0 && definition.CompareRotatedVariants(0, rotation)) {
					continue;
				}

				variants.Add(new BakedModuleVariant {
					StableId = definition.StableId,
					Name = (definition.Prefab != null ? definition.Prefab.name : definition.name) + " R" + rotation,
					Source = definition,
					Prefab = definition.Prefab,
					Rotation = rotation,
					Index = index++,
					Probability = definition.Probability,
					Spawn = definition.Spawn,
					IsInterior = definition.IsInterior,
					Left = RotateFace(definition, 0, rotation),
					Down = RotateFace(definition, 1, rotation),
					Back = RotateFace(definition, 2, rotation),
					Right = RotateFace(definition, 3, rotation),
					Up = RotateFace(definition, 4, rotation),
					Forward = RotateFace(definition, 5, rotation)
				});
			}
		}

		return variants;
	}

	public static BakedAdjacencyData BuildAdjacency(IReadOnlyList<BakedModuleVariant> variants) {
		var adjacency = new BakedAdjacencyData {
			DirectionCount = 6,
			ModuleCount = variants.Count,
			AllowedNeighborIndices = new BakedNeighborSet[variants.Count * 6]
		};

		for (int moduleIndex = 0; moduleIndex < variants.Count; moduleIndex++) {
			var module = variants[moduleIndex];
			for (int direction = 0; direction < 6; direction++) {
				var face = module.GetFace(direction);
				adjacency.AllowedNeighborIndices[moduleIndex * 6 + direction] = new BakedNeighborSet {
					Indices = variants
						.Where(neighbor => Fits(module, direction, neighbor)
						&& !IsExcluded(face, neighbor.Source)
						&& !IsExcluded(neighbor.GetFace((direction + 3) % 6), module.Source)
						&& (!face.EnforceWalkableNeighbor || neighbor.GetFace((direction + 3) % 6).Walkable)
						&& (face.Walkable || !neighbor.GetFace((direction + 3) % 6).EnforceWalkableNeighbor))
					.Select(neighbor => neighbor.Index)
					.ToArray()
				};
			}
		}

		return adjacency;
	}

	public static string ComputeFingerprint(ModuleCatalogSO catalog) {
		var parts = new List<string>();
		foreach (var definition in catalog.GetValidItems().OrderBy(item => item.StableId)) {
			parts.Add(definition.StableId);
			parts.Add(definition.Probability.ToString("R"));
			parts.Add(definition.Spawn ? "1" : "0");
			parts.Add(definition.IsInterior ? "1" : "0");
			parts.Add(definition.AllowRotations ? "1" : "0");
			foreach (var face in definition.GetFaces()) {
				parts.Add(face.Connector.ToString());
				parts.Add(face.Walkable ? "1" : "0");
				parts.Add(face.EnforceWalkableNeighbor ? "1" : "0");
				parts.Add(face.IsOcclusionPortal ? "1" : "0");
			}
		}

		return string.Join("|", parts.ToArray());
	}

	private static void SimplifyAdjacency(ModuleBakeDataSO target) {
		var runtimeModules = LegacyModuleRuntimeAdapter.CreateLegacyModules(target);
		const int height = 12;
		var center = new Vector3Int(0, height / 2, 0);

		for (int moduleIndex = 0; moduleIndex < runtimeModules.Length; moduleIndex++) {
			var module = runtimeModules[moduleIndex];
			var map = new InfiniteMap(height);
			var slot = map.GetSlot(center);
			try {
				slot.Collapse(module);
			} catch (CollapseFailedException exception) {
				Debug.LogWarning(
					"Skipping simplify pass for boundary-dependent module "
					+ module.Name
					+ ". It creates a failure at relative position "
					+ (exception.Slot.Position - center)
					+ ". Keeping base adjacency for this module.");
				continue;
			}

			for (int direction = 0; direction < 6; direction++) {
				var neighbor = slot.GetNeighbor(direction);
				target.Adjacency.AllowedNeighborIndices[moduleIndex * 6 + direction] = new BakedNeighborSet {
					Indices = module.PossibleNeighbors[direction]
						.Where(possibleNeighbor => neighbor.Modules.Contains(possibleNeighbor))
						.Select(possibleNeighbor => possibleNeighbor.Index)
						.ToArray()
				};
			}
		}
	}

	private static bool Fits(BakedModuleVariant module, int direction, BakedModuleVariant neighbor) {
		int otherDirection = (direction + 3) % 6;
		if (Orientations.IsHorizontal(direction)) {
			var face = module.GetFace(direction);
			var otherFace = neighbor.GetFace(otherDirection);
			return face.Connector == otherFace.Connector && (face.Symmetric || face.Flipped != otherFace.Flipped);
		}

		var verticalFace = module.GetFace(direction);
		var otherVerticalFace = neighbor.GetFace(otherDirection);
		return verticalFace.Connector == otherVerticalFace.Connector
			&& (verticalFace.Invariant || (verticalFace.Rotation + module.Rotation) % 4 == (otherVerticalFace.Rotation + neighbor.Rotation) % 4);
	}

	private static bool IsExcluded(BakedFaceData face, ModuleDefinitionSO definition) {
		return definition != null
			&& face.ExcludedStableIds != null
			&& face.ExcludedStableIds.Contains(definition.StableId);
	}

	private static BakedFaceData RotateFace(ModuleDefinitionSO definition, int direction, int rotation) {
		int sourceDirection = Orientations.IsHorizontal(direction) ? Orientations.Rotate(direction, rotation) : direction;
		return BakedFaceData.FromAuthoring(definition.GetFace(sourceDirection));
	}

	private static ModuleBakeDataSO GetOrCreateBakeAsset(ModuleCatalogSO catalog) {
		string catalogPath = AssetDatabase.GetAssetPath(catalog);
		string directory = System.IO.Path.GetDirectoryName(catalogPath);
		string bakePath = System.IO.Path.Combine(directory, catalog.name + ".Baked.asset").Replace("\\", "/");

		var asset = AssetDatabase.LoadAssetAtPath<ModuleBakeDataSO>(bakePath);
		if (asset != null) {
			return asset;
		}

		asset = ScriptableObject.CreateInstance<ModuleBakeDataSO>();
		AssetDatabase.CreateAsset(asset, bakePath);
		return asset;
	}
}
