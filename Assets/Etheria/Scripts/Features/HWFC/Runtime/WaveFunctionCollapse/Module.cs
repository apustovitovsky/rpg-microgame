using UnityEngine;
using System;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Etheria.Features.HWFC {

[Serializable]
public class Module {
	public string Name;

	public ModulePrototype Prototype;
	public GameObject Prefab;
	public float Probability;
	public bool Spawn;
	public bool IsInterior;
	public ModuleFaceDetails[] Faces;
	public bool FacesArePreRotated;

	public int Rotation;

	public ModuleSet[] PossibleNeighbors;
	public Module[][] PossibleNeighborsArray;

	[HideInInspector]
	public int Index;

	// This is precomputed to make entropy calculation faster
	public float PLogP;

	public Module(GameObject prefab, int rotation, int index) {
		Rotation = rotation;
		Index = index;
		Prefab = prefab;
		Prototype = Prefab.GetComponent<ModulePrototype>();
		Name = Prefab.name + " R" + rotation;
		Probability = Prototype.Probability;
		Spawn = Prototype.Spawn;
		IsInterior = Prototype.IsInterior;
		Faces = CreateFacesFromPrototype(Prototype);
		FacesArePreRotated = false;
		PLogP = Probability * Mathf.Log(Probability);
	}

	public Module(BakedModuleVariant variant) {
		Rotation = variant.Rotation;
		Index = variant.Index;
		Prefab = variant.Prefab;
		Name = variant.Name;
		Probability = variant.Probability;
		Spawn = variant.Spawn;
		IsInterior = variant.IsInterior;
		Faces = new ModuleFaceDetails[] {
			createHorizontalFace(variant.Left),
			createVerticalFace(variant.Down),
			createHorizontalFace(variant.Back),
			createHorizontalFace(variant.Right),
			createVerticalFace(variant.Up),
			createHorizontalFace(variant.Forward)
		};
		FacesArePreRotated = true;
		PLogP = Probability * Mathf.Log(Probability);
	}

	public bool Fits(int direction, Module module) {
		int otherDirection = (direction + 3) % 6;

		if (Orientations.IsHorizontal(direction)) {
			var f1 = GetFace(direction) as HorizontalModuleFaceDetails;
			var f2 = module.GetFace(otherDirection) as HorizontalModuleFaceDetails;
			return f1.Connector == f2.Connector && (f1.Symmetric || f1.Flipped != f2.Flipped);
		} else {
			var f1 = GetFace(direction) as VerticalModuleFaceDetails;
			var f2 = module.GetFace(otherDirection) as VerticalModuleFaceDetails;
			return f1.Connector == f2.Connector && (f1.Invariant || (f1.Rotation + Rotation) % 4 == (f2.Rotation + module.Rotation) % 4);
		}
	}

	public bool Fits(int direction, int connector) {
		if (Orientations.IsHorizontal(direction)) {
			var f = GetFace(direction) as HorizontalModuleFaceDetails;
			return f.Connector == connector;
		} else {
			var f = GetFace(direction) as VerticalModuleFaceDetails;
			return f.Connector == connector;
		}
	}

	public ModuleFaceDetails GetFace(int direction) {
		return Faces[FacesArePreRotated ? direction : Orientations.Rotate(direction, Rotation)];
	}

	public override string ToString() {
		return Name;
	}

	private ModuleFaceDetails[] CreateFacesFromPrototype(ModulePrototype prototype) {
		return prototype.Faces.Select(face => CreateFace(face)).ToArray();
	}

	private ModuleFaceDetails CreateFace(ModulePrototype.FaceDetails face) {
		if (face is ModulePrototype.HorizontalFaceDetails horizontal) {
			return new HorizontalModuleFaceDetails {
				Walkable = horizontal.Walkable,
				Connector = horizontal.Connector,
				ExcludedNeighbours = horizontal.ExcludedNeighbours ?? Array.Empty<ModulePrototype>(),
				EnforceWalkableNeighbor = horizontal.EnforceWalkableNeighbor,
				IsOcclusionPortal = horizontal.IsOcclusionPortal,
				Symmetric = horizontal.Symmetric,
				Flipped = horizontal.Flipped
			};
		}

		var vertical = face as ModulePrototype.VerticalFaceDetails;
		return new VerticalModuleFaceDetails {
			Walkable = vertical.Walkable,
			Connector = vertical.Connector,
			ExcludedNeighbours = vertical.ExcludedNeighbours ?? Array.Empty<ModulePrototype>(),
			EnforceWalkableNeighbor = vertical.EnforceWalkableNeighbor,
			IsOcclusionPortal = vertical.IsOcclusionPortal,
			Invariant = vertical.Invariant,
			Rotation = vertical.Rotation
		};
	}

	private static HorizontalModuleFaceDetails createHorizontalFace(BakedFaceData face) {
		return new HorizontalModuleFaceDetails {
			Walkable = face.Walkable,
			Connector = face.Connector,
			EnforceWalkableNeighbor = face.EnforceWalkableNeighbor,
			IsOcclusionPortal = face.IsOcclusionPortal,
			Symmetric = face.Symmetric,
			Flipped = face.Flipped
		};
	}

	private static VerticalModuleFaceDetails createVerticalFace(BakedFaceData face) {
		return new VerticalModuleFaceDetails {
			Walkable = face.Walkable,
			Connector = face.Connector,
			EnforceWalkableNeighbor = face.EnforceWalkableNeighbor,
			IsOcclusionPortal = face.IsOcclusionPortal,
			Invariant = face.Invariant,
			Rotation = face.Rotation
		};
	}
}
}
