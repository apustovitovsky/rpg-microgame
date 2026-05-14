using System;

namespace Etheria.Features.HWFC {

[Serializable]
public class BakedNeighborSet {
	public int[] Indices = Array.Empty<int>();
}

[Serializable]
public class BakedAdjacencyData {
	public int DirectionCount = 6;
	public int ModuleCount;
	public BakedNeighborSet[] AllowedNeighborIndices = Array.Empty<BakedNeighborSet>();
}
}
