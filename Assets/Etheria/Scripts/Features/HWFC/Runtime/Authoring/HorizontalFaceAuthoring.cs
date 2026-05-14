using System;

namespace Etheria.Features.HWFC {

[Serializable]
public class HorizontalFaceAuthoring : FaceAuthoringBase {
	public bool Symmetric;
	public bool Flipped;

	public override string ToString() {
		return Connector.ToString() + (Symmetric ? "s" : (Flipped ? "F" : ""));
	}
}
}
