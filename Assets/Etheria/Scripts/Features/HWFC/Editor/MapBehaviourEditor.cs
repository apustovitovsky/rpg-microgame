
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using Etheria.Features.HWFC;


[CustomEditor(typeof(MapBehaviour))]
public class MapBehaviourEditor : Editor
{
	private int collapseAreaSize = 6;

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		MapBehaviour mapBehaviour = (MapBehaviour)target;
		if (GUILayout.Button("Clear"))
		{
			mapBehaviour.Clear();
			EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
		}

		GUILayout.BeginHorizontal();
		int.TryParse(GUILayout.TextField(collapseAreaSize.ToString()), out collapseAreaSize);

		if (GUILayout.Button("Initialize " + collapseAreaSize + "x" + collapseAreaSize + " area"))
		{
			mapBehaviour.Initialize();
			var startTime = System.DateTime.Now;
			mapBehaviour.Map.Collapse(Vector3Int.zero, new Vector3Int(collapseAreaSize, mapBehaviour.Map.Height, collapseAreaSize), true);
			Debug.Log("Initialized in " + (System.DateTime.Now - startTime).TotalSeconds + " seconds.");
			mapBehaviour.BuildAllSlots();
			GUIUtility.ExitGUI();
		}
		GUILayout.EndHorizontal();
	}
}
