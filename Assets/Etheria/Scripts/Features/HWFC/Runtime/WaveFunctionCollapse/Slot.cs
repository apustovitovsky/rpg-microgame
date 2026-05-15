namespace Etheria.Features.HWFC
{
	using System;
	using System.Linq;
	using UnityEngine;

	public class Slot
	{
		public Vector3Int Position;

		// List of modules that can still be placed here
		public ModuleSet Modules;

		// Direction -> Module -> Number of items in this.getneighbor(direction).Modules that allow this module as a neighbor
		public short[][] ModuleHealth;

		private readonly AbstractMap map;

		public Module Module;

		public GameObject GameObject;

		public bool Collapsed
		{
			get
			{
				return Module != null;
			}
		}

		public bool ConstructionComplete
		{
			get
			{
				return GameObject != null || (Collapsed && !Module.Spawn);
			}
		}

		public Slot(Vector3Int position, AbstractMap map)
		{
			Position = position;
			this.map = map;
			ModuleHealth = map.CopyInititalModuleHealth();
			Modules = new ModuleSet(initializeFull: true);
		}

		public Slot(Vector3Int position, AbstractMap map, Slot prototype)
		{
			Position = position;
			this.map = map;
			ModuleHealth = prototype.ModuleHealth.Select(a => a.ToArray()).ToArray();
			Modules = new ModuleSet(prototype.Modules);
		}

		// TODO only look up once and then cache???
		public Slot GetNeighbor(int direction)
		{
			return map.GetSlot(Position + Orientations.Direction[direction]);
		}

		public void Collapse(Module module)
		{
			if (Collapsed)
			{
				Debug.LogWarning("Trying to collapse already collapsed slot.");
				return;
			}

			map.History.Push(new HistoryItem(this));

			Module = module;
			var toRemove = new ModuleSet(Modules);
			toRemove.Remove(module);
			RemoveModules(toRemove);

			map.NotifySlotCollapsed(this);
		}

		private void checkConsistency(Module module)
		{
			for (int d = 0; d < 6; d++)
			{
				if (GetNeighbor(d) != null && GetNeighbor(d).Collapsed && !GetNeighbor(d).Module.PossibleNeighbors[(d + 3) % 6].Contains(module))
				{
					throw new Exception("Illegal collapse, not in neighbour list. (Incompatible connectors)");
				}
			}

			if (!Modules.Contains(module))
			{
				throw new Exception("Illegal collapse!");
			}
		}

		public void CollapseRandom()
		{
			if (!Modules.Any())
			{
				throw new CollapseFailedException(this);
			}
			if (Collapsed)
			{
				throw new Exception("Slot is already collapsed.");
			}

			float max = Modules.Select(module => module.Probability).Sum();
			float roll = (float)(InfiniteMap.Random.NextDouble() * max);
			float p = 0;
			foreach (var candidate in Modules)
			{
				p += candidate.Probability;
				if (p >= roll)
				{
					Collapse(candidate);
					return;
				}
			}
			Collapse(Modules.First());
		}

		// This modifies the supplied ModuleSet as a side effect
		public void RemoveModules(ModuleSet modulesToRemove, bool recursive = true)
		{
			modulesToRemove.Intersect(Modules);

			if (map.History != null && map.History.Any())
			{
				var item = map.History.Peek();
				if (!item.RemovedModules.ContainsKey(Position))
				{
					item.RemovedModules[Position] = new ModuleSet();
				}
				item.RemovedModules[Position].Add(modulesToRemove);
			}

			for (int d = 0; d < 6; d++)
			{
				int inverseDirection = (d + 3) % 6;
				var neighbor = GetNeighbor(d);
				if (neighbor == null || neighbor.Forgotten)
				{
#if UNITY_EDITOR
					if (map is InfiniteMap && (map as InfiniteMap).IsOutsideOfRangeLimit(Position + Orientations.Direction[d]))
					{
						(map as InfiniteMap).OnHitRangeLimit(Position + Orientations.Direction[d], modulesToRemove);
					}
#endif
					continue;
				}

				foreach (var module in modulesToRemove)
				{
					for (int i = 0; i < module.PossibleNeighborsArray[d].Length; i++)
					{
						var possibleNeighbor = module.PossibleNeighborsArray[d][i];
						if (neighbor.ModuleHealth[inverseDirection][possibleNeighbor.Index] == 1 && neighbor.Modules.Contains(possibleNeighbor))
						{
							map.RemovalQueue[neighbor.Position].Add(possibleNeighbor);
						}
#if UNITY_EDITOR
						if (neighbor.ModuleHealth[inverseDirection][possibleNeighbor.Index] < 1)
						{
							throw new System.InvalidOperationException("ModuleHealth must not be negative. " + Position + " d: " + d);
						}
#endif
						neighbor.ModuleHealth[inverseDirection][possibleNeighbor.Index]--;
					}
				}
			}

			Modules.Remove(modulesToRemove);

			if (Modules.Empty)
			{
				throw new CollapseFailedException(this);
			}

			if (recursive)
			{
				map.FinishRemovalQueue();
			}
		}

		/// <summary>
		/// Add modules non-recursively.
		/// Returns true if this lead to this slot changing from collapsed to not collapsed.
		/// </summary>
		public void AddModules(ModuleSet modulesToAdd)
		{
			foreach (var module in modulesToAdd)
			{
				if (Modules.Contains(module) || module == Module)
				{
					continue;
				}
				for (int d = 0; d < 6; d++)
				{
					int inverseDirection = (d + 3) % 6;
					var neighbor = GetNeighbor(d);
					if (neighbor == null || neighbor.Forgotten)
					{
						continue;
					}

					foreach (var possibleNeighbor in module.PossibleNeighbors[d])
					{
						neighbor.ModuleHealth[inverseDirection][possibleNeighbor.Index]++;
					}
				}
				Modules.Add(module);
			}

			if (Collapsed && !Modules.Empty)
			{
				Module = null;
				map.NotifySlotCollapseUndone(this);
			}
		}

		public void EnforceConnector(int direction, int connector)
		{
			var toRemove = Modules.Where(module => !module.Fits(direction, connector));
			RemoveModules(ModuleSet.FromEnumerable(toRemove));
		}

		public void ExcludeConnector(int direction, int connector)
		{
			var toRemove = Modules.Where(module => module.Fits(direction, connector));
			RemoveModules(ModuleSet.FromEnumerable(toRemove));
		}

		public override int GetHashCode()
		{
			return Position.GetHashCode();
		}

		public void Forget()
		{
			ModuleHealth = null;
			Modules = null;
		}

		public bool Forgotten
		{
			get
			{
				return Modules == null;
			}
		}
	}
}
