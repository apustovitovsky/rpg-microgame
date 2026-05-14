using UnityEngine;
using System;

namespace Etheria.Features.HWFC
{
	[Serializable]
	public struct Module
	{

		public int Rotation;
		public ModuleSet[] PossibleNeighbors;
		public Module[][] PossibleNeighborsArray;

		[HideInInspector]
		public int Index;

		// This is precomputed to make entropy calculation faster
		public float PLogP;

		public bool Fits(int direction, Module module)
		{
			throw new NotImplementedException();
		}

		public bool Fits(int direction, int connector)
		{
			throw new NotImplementedException();
		}
	}
}