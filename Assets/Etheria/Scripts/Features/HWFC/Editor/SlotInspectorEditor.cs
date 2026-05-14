using UnityEditor;
using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;
using Etheria.Features.HWFC;


[CustomEditor(typeof(SlotInspector))]
public class SlotInspectorEditor : Editor {
	private string filterString = "";

	private void ShowEditor(Slot slot, MapBehaviour mapBehaviour) {
		if (slot.Collapsed) {
			GUILayout.Label("Collapsed: " + slot.Module);
			GUILayout.Space(20f);
			if (slot.Module.Prototype != null) {
				GUILayout.Label("Add exclusion rules:");
				CreateNeighborExlusionUI(slot, mapBehaviour);
			} else {
				EditorGUILayout.HelpBox("Exclusion editing is only available for legacy ModulePrototype-based authoring.", MessageType.Info);
			}
			return;
		}

		GUILayout.Label("Possible modules: " + slot.Modules.Count() + " / " + ModuleData.Current.Count());
		GUILayout.Label("Entropy: " + slot.Modules.Entropy);

		if (GUILayout.Button("Collapse Random")) {
			slot.CollapseRandom();
			mapBehaviour.BuildAllSlots();
		}

		var modulesByPrefab = new Dictionary<GameObject, List<Module>>();

		foreach (var module in slot.Modules.ToArray()) {
			var prefab = module.Prefab;
			if (!modulesByPrefab.ContainsKey(prefab)) {
				modulesByPrefab[prefab] = new List<Module>();
			}
			modulesByPrefab[prefab].Add(module);
		}

		if (modulesByPrefab.Any()) {
			GUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Filter: ");
			filterString = GUILayout.TextField(filterString);
			GUILayout.EndHorizontal();
		}

		int hiddenByFilter = 0;
		foreach (var prefab in modulesByPrefab.Keys) {
			var prefabName = prefab != null ? prefab.name : "(Missing Prefab)";
			if (filterString != "" && !prefabName.ToLower().Contains(filterString.ToLower())) {
				hiddenByFilter++;
				continue;
			}

			var list = modulesByPrefab[prefab];

			GUILayout.BeginHorizontal();

			EditorGUILayout.PrefixLabel(prefabName + " (" + (100f * list.Sum(module => module.Probability) / slot.Modules.Sum(module => module.Probability)).ToString("0.0") + "%)");
			foreach (var module in list) {
				if (GUILayout.Button("R" + module.Rotation)) {
					slot.Collapse(module);
					mapBehaviour.BuildAllSlots();
				}
			}

			GUILayout.EndHorizontal();
		}

		if (hiddenByFilter > 0) {
			GUILayout.Label("(+" + hiddenByFilter + " that don't match the filter query)");
		}

		var defaultSlot = mapBehaviour.Map.GetDefaultSlot(slot.Position.y);
		var removedPrefabs = new List<GameObject>();
		var removedByDefault = new List<GameObject>();

		foreach (var module in ModuleData.Current) {
			if (!modulesByPrefab.ContainsKey(module.Prefab)) {
				modulesByPrefab.Add(module.Prefab, null);
				if (defaultSlot != null && !defaultSlot.Modules.Contains(module)) {
					removedByDefault.Add(module.Prefab);
				} else {
					removedPrefabs.Add(module.Prefab);
				}
			}
		}

		if (removedPrefabs.Any()) {
			GUILayout.Space(15f);
			GUILayout.Label("Removed modules:");
			foreach (var prefab in removedPrefabs) {
				GUILayout.Label(prefab != null ? prefab.name : "(Missing Prefab)");
			}
		}
		if (removedByDefault.Any()) {
			GUILayout.Space(15f);
			GUILayout.Label("Modules always removed at this y coordinate:");
			foreach (var prefab in removedByDefault) {
				GUILayout.Label(prefab != null ? prefab.name : "(Missing Prefab)");
			}
		}
	}

	public override void OnInspectorGUI() {
		DrawDefaultInspector();

		SlotInspector slotInspector = (SlotInspector)target;
		var mapBehaviour = slotInspector.MapBehaviour;
		if (mapBehaviour == null) {
			return;
		}
		var map = mapBehaviour.Map;

		var position = slotInspector.MapBehaviour.GetMapPosition(slotInspector.transform.position);
		GUILayout.Label("Position: " + position);

		if (!mapBehaviour.Initialized) {
			if (GUILayout.Button("Initialize Map")) {
				mapBehaviour.Initialize();
				mapBehaviour.Map.GetSlot(position);
			}
			return;
		}

		if (GUILayout.Button("Reset Map")) {
			mapBehaviour.Clear();
			mapBehaviour.Initialize();
			mapBehaviour.Map.GetSlot(position);
		}

		if (mapBehaviour.Map.History.Any() && GUILayout.Button("Undo Last Collapse")) {
			DestroyImmediate(mapBehaviour.Map.History.Peek().Slot.GameObject);
			mapBehaviour.Map.Undo(1);
		}

		GUILayout.Space(10f);

		if (map.IsSlotInitialized(position)) {
			ShowEditor(map.GetSlot(position), mapBehaviour);
		} else {
			if (GUILayout.Button("Create Slot")) {
				map.GetSlot(position);
			}
		}
	}

	public void OnSceneGUI() {
		SlotInspector slotInspector = (SlotInspector)target;
		if (slotInspector.MapBehaviour == null) {
			return;
		}
		slotInspector.transform.position = slotInspector.MapBehaviour.GetWorldspacePosition(slotInspector.MapBehaviour.GetMapPosition(slotInspector.transform.position));
	}

	private void CreateNeighborExlusionUI(Slot slot, MapBehaviour mapBehaviour) {
		var style = new GUIStyle();

		for (int i = 0; i < 6; i++) {
			GUILayout.Space(10f);
			style.normal.textColor = GetColor(i);
			var neighbor = slot.GetNeighbor(i);

			GUILayout.Label(Orientations.Names[i], style);
			if (neighbor == null || !neighbor.Collapsed) {
				GUILayout.Label("(No neighbor)");
				continue;
			}

			GUILayout.Label(neighbor.Module.ToString());

			if (neighbor.Module == null) {
				continue;
			}

			var ownFace = slot.Module.GetFace(i);
			var neighborFace = neighbor.Module.GetFace((i + 3) % 6);

			if (ownFace.ExcludedNeighbours.Contains(neighbor.Module.Prototype) && neighborFace.ExcludedNeighbours.Contains(slot.Module.Prototype)) {
				GUILayout.Label("(Already exlcuded)");
				continue;
			}

			if (GUILayout.Button("Exclude neighbor")) {
				if (!ownFace.ExcludedNeighbours.Contains(neighbor.Module.Prototype)) {
					ownFace.ExcludedNeighbours = ownFace.ExcludedNeighbours.Concat(new ModulePrototype[] { neighbor.Module.Prototype }).ToArray();
				}
				if (!neighborFace.ExcludedNeighbours.Contains(slot.Module.Prototype)) {
					neighborFace.ExcludedNeighbours = neighborFace.ExcludedNeighbours.Concat(new ModulePrototype[] { slot.Module.Prototype }).ToArray();
				}

				EditorUtility.SetDirty(slot.Module.Prototype);
				EditorUtility.SetDirty(neighbor.Module.Prototype);
				AssetDatabase.SaveAssets();
				Debug.Log("Added exclusion rule.");
			}

			if (neighborFace.Walkable) {
				GUILayout.Label("(Neighbor is walkable)");
				continue;
			}

			if (ownFace.EnforceWalkableNeighbor && !neighborFace.Walkable) {
				GUILayout.Label("(Already exlcuded by walkability constraint)");
				continue;
			}

			if (!ownFace.EnforceWalkableNeighbor && !neighborFace.Walkable && GUILayout.Button("Enforce Walkable neighbor")) {
				ownFace.EnforceWalkableNeighbor = true;
				EditorUtility.SetDirty(slot.Module.Prototype);
				AssetDatabase.SaveAssets();
				Debug.Log("Added exclusion rule.");
			}
		}
	}

	private Color GetColor(int direction) {
        return direction switch
        {
            0 => Color.red,
            1 => Color.green,
            2 => Color.blue,
            3 => Color.red,
            4 => Color.green,
            5 => Color.blue,
            _ => throw new NotImplementedException(),
        };
    }
}
