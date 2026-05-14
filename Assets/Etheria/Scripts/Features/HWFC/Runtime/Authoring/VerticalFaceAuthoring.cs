using System;
using System.Linq;

namespace Etheria.Features.HWFC {

[Serializable]
public class VerticalFaceAuthoring : FaceAuthoringBase {
	public bool Invariant;
	public int Rotation;

	public override string ToString() {
		return Connector.ToString() + (Invariant ? "i" : (Rotation != 0 ? "_bcd".ElementAt(Rotation).ToString() : ""));
	}
}
}
