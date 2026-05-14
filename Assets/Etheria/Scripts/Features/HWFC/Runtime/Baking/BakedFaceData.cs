using System;
using System.Linq;

namespace Etheria.Features.HWFC {

[Serializable]
public class BakedFaceData {
	public int Connector;
	public bool Walkable;
	public bool EnforceWalkableNeighbor;
	public bool IsOcclusionPortal;
	public bool Symmetric;
	public bool Flipped;
	public bool Invariant;
	public int Rotation;
	public string[] ExcludedStableIds = Array.Empty<string>();

	public static BakedFaceData FromAuthoring(FaceAuthoringBase face) {
		if (face == null) {
			throw new ArgumentNullException(nameof(face));
		}

		var result = new BakedFaceData {
			Connector = face.Connector,
			Walkable = face.Walkable,
			EnforceWalkableNeighbor = face.EnforceWalkableNeighbor,
			IsOcclusionPortal = face.IsOcclusionPortal,
			ExcludedStableIds = (face.ExcludedNeighbours ?? Array.Empty<ModuleDefinitionSO>())
				.WhereNotNull()
				.Select(item => item.StableId)
				.Where(id => !string.IsNullOrWhiteSpace(id))
				.Distinct()
				.ToArray()
		};

		if (face is HorizontalFaceAuthoring horizontal) {
			result.Symmetric = horizontal.Symmetric;
			result.Flipped = horizontal.Flipped;
		} else if (face is VerticalFaceAuthoring vertical) {
			result.Invariant = vertical.Invariant;
			result.Rotation = vertical.Rotation;
		}

		return result;
	}

	public BakedFaceData Clone() {
		return new BakedFaceData {
			Connector = Connector,
			Walkable = Walkable,
			EnforceWalkableNeighbor = EnforceWalkableNeighbor,
			IsOcclusionPortal = IsOcclusionPortal,
			Symmetric = Symmetric,
			Flipped = Flipped,
			Invariant = Invariant,
			Rotation = Rotation,
			ExcludedStableIds = (ExcludedStableIds ?? Array.Empty<string>()).ToArray()
		};
	}

	public override string ToString() {
		if (Invariant) {
			return Connector + "i";
		}
		if (Symmetric) {
			return Connector + "s";
		}
		if (Flipped) {
			return Connector + "F";
		}
		if (Rotation != 0) {
			return Connector + "_" + Rotation;
		}
		return Connector.ToString();
	}
}

static class BakedFaceDataExtensions {
	public static System.Collections.Generic.IEnumerable<T> WhereNotNull<T>(this System.Collections.Generic.IEnumerable<T> source) where T : class =>
		source.Where(item => item != null);
}
}
