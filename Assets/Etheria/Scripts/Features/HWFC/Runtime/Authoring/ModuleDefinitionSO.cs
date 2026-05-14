using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Etheria.Features.HWFC {

[CreateAssetMenu(menuName = "Etheria/HWFC/Module Definition", fileName = "ModuleDefinition")]
public class ModuleDefinitionSO : ScriptableObject {
	public string StableId;
	public GameObject Prefab;
	public float Probability = 1f;
	public bool Spawn = true;
	public bool IsInterior;
	public bool AllowRotations = true;

	public HorizontalFaceAuthoring Left = new();
	public VerticalFaceAuthoring Down = new();
	public HorizontalFaceAuthoring Back = new();
	public HorizontalFaceAuthoring Right = new();
	public VerticalFaceAuthoring Up = new();
	public HorizontalFaceAuthoring Forward = new();

	public FaceAuthoringBase GetFace(int direction) {
            return direction switch
            {
                0 => Left,
                1 => Down,
                2 => Back,
                3 => Right,
                4 => Up,
                5 => Forward,
                _ => throw new ArgumentOutOfRangeException(nameof(direction)),
            };
        }

	public FaceAuthoringBase[] GetFaces() {
		return new FaceAuthoringBase[] {
			Left,
			Down,
			Back,
			Right,
			Up,
			Forward
		};
	}

	public bool CompareRotatedVariants(int r1, int r2) {
		if (!Up.Invariant || !Down.Invariant) {
			return false;
		}

		for (int i = 0; i < 4; i++) {
			var face1 = (HorizontalFaceAuthoring)GetFace(Orientations.Rotate(Orientations.HorizontalDirections[i], r1));
			var face2 = (HorizontalFaceAuthoring)GetFace(Orientations.Rotate(Orientations.HorizontalDirections[i], r2));

			if (face1.Connector != face2.Connector) {
				return false;
			}

			if (!face1.Symmetric && !face2.Symmetric && face1.Flipped != face2.Flipped) {
				return false;
			}
		}

		return true;
	}

	public void OnValidate() {
		Probability = Mathf.Max(0.0001f, Probability);
		Down.Rotation = ((Down.Rotation % 4) + 4) % 4;
		Up.Rotation = ((Up.Rotation % 4) + 4) % 4;
#if UNITY_EDITOR
		if (string.IsNullOrWhiteSpace(StableId)) {
			StableId = GUID.Generate().ToString();
			EditorUtility.SetDirty(this);
		}
#endif
	}
}
}
