using System;

namespace Etheria.Features.HWFC {

[Serializable]
public abstract class FaceAuthoringBase {
	public bool Walkable;
	public int Connector;
	public bool EnforceWalkableNeighbor;
	public bool IsOcclusionPortal;
	public ModuleDefinitionSO[] ExcludedNeighbours = Array.Empty<ModuleDefinitionSO>();
}
}
