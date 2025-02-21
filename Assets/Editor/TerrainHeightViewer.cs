#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class TerrainHeightViewer 
{
    static GameObject targetObject = GameObject.Find("car");

    [InitializeOnLoadMethod]
    static void Init()
    {
        Debug.Log("TerrainHeightViewer initialized");
        SceneView.duringSceneGui += OnSceneGUI;
    }

    static void OnSceneGUI(SceneView sceneView)
    {   
        if (targetObject == null)
        {
            Debug.Log("No GameObject found");
        }
        Event e = Event.current;     
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.red;
            Handles.Label(hit.point, $"Position: {hit.point}", style);
            Handles.DrawWireCube(hit.point, Vector3.one * 0.5f);
        }

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            Debug.Log($"Click position: {hit.point}");
            Undo.RecordObject(targetObject.transform, "Move Object");  // Allows for undo
            targetObject.transform.position = hit.point + Vector3.up * 5;
            Debug.Log($"Moved object to position: {hit.point}");
        }
    }
}
#endif