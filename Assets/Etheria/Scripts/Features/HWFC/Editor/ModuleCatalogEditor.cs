using System;
using System.Linq;
using Etheria.Features.HWFC;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ModuleCatalogSO))]
public class ModuleCatalogEditor : Editor {
	public override void OnInspectorGUI() {
		DrawDefaultInspector();

		var catalog = (ModuleCatalogSO)target;
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Definitions", catalog.GetValidItems().Count().ToString());

		if (GUILayout.Button("Validate")) {
			var errors = ModuleValidation.ValidateCatalog(catalog);
			if (errors.Any()) {
				Debug.LogError(string.Join("\n", errors.ToArray()), catalog);
			} else {
				Debug.Log("Catalog validation passed.", catalog);
			}
		}

		if (GUILayout.Button("Bake")) {
			BakeCatalog(catalog, false);
		}

		if (GUILayout.Button("Bake Simplified")) {
			BakeCatalog(catalog, true);
		}
	}

	private void BakeCatalog(ModuleCatalogSO catalog, bool simplify) {
		try {
			var bakeData = ModuleBaker.Bake(catalog, null, simplify);
			Debug.Log("Baked module data to " + AssetDatabase.GetAssetPath(bakeData), bakeData);
			EditorGUIUtility.PingObject(bakeData);
		} catch (Exception exception) {
			Debug.LogException(exception, catalog);
		}
	}
}
