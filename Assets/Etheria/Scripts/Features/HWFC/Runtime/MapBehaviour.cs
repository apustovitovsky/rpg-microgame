
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Linq;
using System;

namespace Etheria.Features.HWFC
{
	public class MapBehaviour : MonoBehaviour
	{
		public InfiniteMap Map;

		public int MapHeight = 24;

		public BoundaryConstraint[] BoundaryConstraints;

		public bool ApplyBoundaryConstraints = true;

		public ModuleBakeDataSO ModuleBakeData;

		private CullingData cullingData;

		public Vector3 GetWorldspacePosition(Vector3Int position)
		{
			return transform.position
				+ Vector3.up * InfiniteMap.BLOCK_SIZE / 2f
				+ position.ToVector3() * InfiniteMap.BLOCK_SIZE;
		}

		public Vector3Int GetMapPosition(Vector3 worldSpacePosition)
		{
			var pos = (worldSpacePosition - transform.position) / InfiniteMap.BLOCK_SIZE;
			return Vector3Int.FloorToInt(pos + new Vector3(0.5f, 0, 0.5f));
		}

		public void Clear()
		{
			var children = new List<Transform>();
			foreach (Transform child in transform)
			{
				children.Add(child);
			}
			foreach (var child in children)
			{
				GameObject.DestroyImmediate(child.gameObject);
			}
			Map = null;
		}

		public void Initialize()
		{
			if (ModuleBakeData == null)
			{
				throw new InvalidOperationException("ModuleBakeData is not assigned.");
			}
			ModuleData.Current = LegacyModuleRuntimeAdapter.CreateLegacyModules(ModuleBakeData);
			Clear();
			Map = new InfiniteMap(MapHeight);
			if (ApplyBoundaryConstraints && BoundaryConstraints != null && BoundaryConstraints.Any())
			{
				Map.ApplyBoundaryConstraints(BoundaryConstraints);
			}
			cullingData = GetComponent<CullingData>();
			if (cullingData == null)
			{
				cullingData = gameObject.AddComponent<CullingData>();
			}
			cullingData.Initialize();
		}

		public bool Initialized
		{
			get
			{
				return Map != null;
			}
		}

		public void Update()
		{
			if (Map == null || Map.BuildQueue == null)
			{
				return;
			}

			int itemsLeft = 50;

			while (Map.BuildQueue.Count != 0 && itemsLeft > 0)
			{
				var slot = Map.BuildQueue.Peek();
				if (slot == null)
				{
					return;
				}
				if (BuildSlot(slot))
				{
					itemsLeft--;
				}
				Map.BuildQueue.Dequeue();
			}
			if (cullingData != null)
			{
				cullingData.ClearOutdatedSlots();
			}
		}

		public bool BuildSlot(Slot slot)
		{
			if (slot.GameObject != null)
			{
				if (cullingData != null)
				{
					cullingData.RemoveSlot(slot);
				}
#if UNITY_EDITOR
				GameObject.DestroyImmediate(slot.GameObject);
#else
			GameObject.Destroy(slot.GameObject);
#endif
			}

			if (!slot.Collapsed || !slot.Module.Spawn)
			{
				return false;
			}
			var module = slot.Module;
			if (module == null)
			{ // Can be null due to race conditions
				return false;
			}

			if (module.Prefab == null)
			{
				return false;
			}

			var gameObject = GameObject.Instantiate(module.Prefab);
			gameObject.name = module.Prefab.name + " " + slot.Position;
			var prototype = gameObject.GetComponent<ModulePrototype>();
			if (prototype != null)
			{
				GameObject.DestroyImmediate(prototype);
			}
			gameObject.transform.parent = transform;
			gameObject.transform.position = GetWorldspacePosition(slot.Position);
			gameObject.transform.rotation = Quaternion.Euler(Vector3.up * 90f * module.Rotation);
			slot.GameObject = gameObject;
			if (cullingData != null)
			{
				cullingData.AddSlot(slot);
			}
			return true;
		}

		public void BuildAllSlots()
		{
			while (Map.BuildQueue.Count != 0)
			{
				BuildSlot(Map.BuildQueue.Dequeue());
			}
		}

		public bool VisualizeSlots = false;

#if UNITY_EDITOR
		[DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
		static void DrawGizmo(MapBehaviour mapBehaviour, GizmoType gizmoType)
		{
			if (!mapBehaviour.VisualizeSlots)
			{
				return;
			}
			if (mapBehaviour.Map == null)
			{
				return;
			}
			foreach (var slot in mapBehaviour.Map.GetAllSlots())
			{
				if (slot.Collapsed || slot.Modules.Count == ModuleData.Current.Length)
				{
					continue;
				}
				Handles.Label(mapBehaviour.GetWorldspacePosition(slot.Position), slot.Modules.Count.ToString());
			}
		}
#endif
	}
}
