using System;
using System.Linq;

namespace Etheria.Features.HWFC {

[Serializable]
public class ModuleFaceDetails {
	public bool Walkable;
	public int Connector;
	public ModulePrototype[] ExcludedNeighbours = Array.Empty<ModulePrototype>();
	public bool EnforceWalkableNeighbor;
	public bool IsOcclusionPortal;

	public virtual string GetDisplaySuffix() {
		return string.Empty;
	}

	public virtual ModuleFaceDetails Clone() {
		return new ModuleFaceDetails {
			Walkable = Walkable,
			Connector = Connector,
			ExcludedNeighbours = (ExcludedNeighbours ?? Array.Empty<ModulePrototype>()).ToArray(),
			EnforceWalkableNeighbor = EnforceWalkableNeighbor,
			IsOcclusionPortal = IsOcclusionPortal
		};
	}

	public override string ToString() {
		return Connector + GetDisplaySuffix();
	}
}

[Serializable]
public class HorizontalModuleFaceDetails : ModuleFaceDetails {
	public bool Symmetric;
	public bool Flipped;

	public override string GetDisplaySuffix() {
		return Symmetric ? "s" : (Flipped ? "F" : string.Empty);
	}

	public override ModuleFaceDetails Clone() {
		return new HorizontalModuleFaceDetails {
			Walkable = Walkable,
			Connector = Connector,
			ExcludedNeighbours = (ExcludedNeighbours ?? Array.Empty<ModulePrototype>()).ToArray(),
			EnforceWalkableNeighbor = EnforceWalkableNeighbor,
			IsOcclusionPortal = IsOcclusionPortal,
			Symmetric = Symmetric,
			Flipped = Flipped
		};
	}
}

[Serializable]
public class VerticalModuleFaceDetails : ModuleFaceDetails {
	public bool Invariant;
	public int Rotation;

	public override string GetDisplaySuffix() {
		return Invariant ? "i" : (Rotation != 0 ? "_" + Rotation : string.Empty);
	}

	public override ModuleFaceDetails Clone() {
		return new VerticalModuleFaceDetails {
			Walkable = Walkable,
			Connector = Connector,
			ExcludedNeighbours = (ExcludedNeighbours ?? Array.Empty<ModulePrototype>()).ToArray(),
			EnforceWalkableNeighbor = EnforceWalkableNeighbor,
			IsOcclusionPortal = IsOcclusionPortal,
			Invariant = Invariant,
			Rotation = Rotation
		};
	}
}
}
