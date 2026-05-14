using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Etheria.Features.HWFC
{
	[Serializable]
	/// <summary>
	/// Represents a single cell in the map. Contains the list of modules that can still be placed here and the number of compatible neighbors for each direction and module.
	/// </summary>
	public struct Slot
	{
		public Vector3Int Position;

		// List of modules that can still be placed here
		public ModuleSet Modules;

		// Direction -> Module -> Number of items in this.getneighbor(direction).Modules that allow this module as a neighbor
		public short[][] ModuleHealth;

		public Module Module;

		public int Version { get; internal set; }
	}
}