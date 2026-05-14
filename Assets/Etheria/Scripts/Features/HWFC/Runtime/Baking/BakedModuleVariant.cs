using System;
using UnityEngine;

namespace Etheria.Features.HWFC {

[Serializable]
public class BakedModuleVariant {
	public string StableId;
	public string Name;
	public ModuleDefinitionSO Source;
	public GameObject Prefab;
	public int Rotation;
	public int Index;
	public float Probability;
	public bool Spawn;
	public bool IsInterior;
	public BakedFaceData Left;
	public BakedFaceData Down;
	public BakedFaceData Back;
	public BakedFaceData Right;
	public BakedFaceData Up;
	public BakedFaceData Forward;

	public BakedFaceData GetFace(int direction) {
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
}
}
