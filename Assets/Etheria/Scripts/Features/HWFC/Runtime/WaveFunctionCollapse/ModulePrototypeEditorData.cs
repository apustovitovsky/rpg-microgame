using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Etheria.Features.HWFC {
public class ModulePrototypeEditorData {
	public readonly ModulePrototype ModulePrototype;

	private readonly ModulePrototype[] prototypes;

	private readonly Dictionary<ModulePrototype, Mesh> meshes;

	public struct ConnectorHint {
		public readonly Mesh Mesh;
		public readonly int Rotation;

		public ConnectorHint(int rotation, Mesh mesh) {
			this.Rotation = rotation;
			this.Mesh = mesh;
		}
	}

	public ModulePrototypeEditorData(ModulePrototype modulePrototype) {
		ModulePrototype = modulePrototype;
		prototypes = new[] { modulePrototype };
		meshes = new Dictionary<ModulePrototype, Mesh>();
	}

	private Mesh getMesh(ModulePrototype modulePrototype) {
		if (modulePrototype == null) {
			return null;
		}

		if (meshes.ContainsKey(modulePrototype)) {
			return meshes[modulePrototype];
		}

		var mesh = modulePrototype.GetMesh(false);
		meshes[modulePrototype] = mesh;
		return mesh;
	}

	public ConnectorHint GetConnectorHint(int direction) {
		if (ModulePrototype == null || ModulePrototype.Faces == null || direction < 0 || direction >= ModulePrototype.Faces.Length) {
			return new ConnectorHint();
		}

		var face = ModulePrototype.Faces[direction];
		if (face == null) {
			return new ConnectorHint();
		}

		if (face is ModulePrototype.HorizontalFaceDetails) {
			var horizontalFace = face as ModulePrototype.HorizontalFaceDetails;
			
			foreach (var prototype in prototypes) {
				if (prototype == null || prototype == ModulePrototype || IsExcluded(face, prototype)) {
					continue;
				}

				for (int rotation = 0; rotation < 4; rotation++) {
					var otherFace = prototype.Faces[Orientations.Rotate(direction, rotation + 2)] as ModulePrototype.HorizontalFaceDetails;
					if (otherFace == null || IsExcluded(otherFace, ModulePrototype)) {
						continue;
					}

					if (otherFace.Connector == face.Connector && ((horizontalFace.Symmetric && otherFace.Symmetric) || otherFace.Flipped != horizontalFace.Flipped)) {
						return new ConnectorHint(rotation, getMesh(prototype));
					}
				}
			}
		}

		if (face is ModulePrototype.VerticalFaceDetails) {
			var verticalFace = face as ModulePrototype.VerticalFaceDetails;

			foreach (var prototype in prototypes) {
				if (prototype == null || prototype == ModulePrototype || IsExcluded(face, prototype)) {
					continue;
				}

				var otherFace = prototype.Faces[(direction + 3) % 6] as ModulePrototype.VerticalFaceDetails;
				if (otherFace == null || IsExcluded(otherFace, ModulePrototype) || otherFace.Connector != face.Connector) {
					continue;
				}

				return new ConnectorHint(verticalFace.Rotation - otherFace.Rotation, getMesh(prototype));
			}
		}

		return new ConnectorHint();
	}

	private static bool IsExcluded(ModulePrototype.FaceDetails face, ModulePrototype prototype) {
		return face != null
			&& face.ExcludedNeighbours != null
			&& prototype != null
			&& face.ExcludedNeighbours.Contains(prototype);
	}
}
}
