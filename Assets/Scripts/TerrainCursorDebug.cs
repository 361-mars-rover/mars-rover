using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class HoverTerrainCursorDebug : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool showDebugInfo = true;
    public Color debugColor = Color.red;
    public float sphereRadius = 0f;
    
    [Header("Display Options")]
    public bool showCoordinates = true;
    public bool showNormal = false;
    public float normalLength = 1.0f;
    
    [Header("Raycast Settings")]
    public LayerMask targetLayers = ~0; // Default to all layers
    public float maxRayDistance = 10000f;
    
    private Vector3 hitPoint;
    private Vector3 hitNormal;
    private bool hasHit = false;
    private string hitObjectName = "";
    
    private void OnEnable()
    {
        // Subscribe to the SceneView update event
        SceneView.duringSceneGui += OnSceneGUI;
    }
    
    private void OnDisable()
    {
        // Unsubscribe from the SceneView update event
        SceneView.duringSceneGui -= OnSceneGUI;
    }
    
    private void OnSceneGUI(SceneView sceneView)
    {
        // Get current event
        Event currentEvent = Event.current;
        
        // We want to detect hover continuously, not just on move events
        if (currentEvent.type == EventType.Repaint || currentEvent.type == EventType.MouseMove)
        {
            // Get mouse position
            Vector2 mousePosition = currentEvent.mousePosition;
            
            // Convert mouse position to ray
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            RaycastHit hit;
            
            // Check if ray hits any collider based on layer mask
            if (Physics.Raycast(ray, out hit, maxRayDistance, targetLayers))
            {
                hitPoint = hit.point;
                hitNormal = hit.normal;
                hasHit = true;
                hitObjectName = hit.collider.gameObject.name;
                
                // Force the scene view to repaint
                sceneView.Repaint();
            }
            else
            {
                hasHit = false;
                
                // Still repaint to hide the debug info
                sceneView.Repaint();
            }
        }
        
        // Display debug information when hovering over a collider
        if (showDebugInfo && hasHit)
        {
            // Draw a sphere at hit point
            Handles.color = debugColor;
            // Handles.SphereHandleCap(0, hitPoint, Quaternion.identity, sphereRadius, EventType.Repaint);
            
            // Draw normal if enabled
            if (showNormal)
            {
                Handles.color = Color.blue;
                Handles.DrawLine(hitPoint, hitPoint + hitNormal * normalLength);
            }
            
            if (showCoordinates)
            {
                // Create style for labels
                GUIStyle style = new GUIStyle();
                style.normal.textColor = debugColor;
                style.fontSize = 12;
                style.fontStyle = FontStyle.Bold;
                style.alignment = TextAnchor.UpperLeft;
                
                // Build info text
                string infoText = $"Position:\nX: {hitPoint.x:F2}\nY: {hitPoint.y:F2}\nZ: {hitPoint.z:F2}";
                infoText += $"\nObject: {hitObjectName}";
                
                if (showNormal)
                {
                    infoText += $"\n\nNormal:\nX: {hitNormal.x:F2}\nY: {hitNormal.y:F2}\nZ: {hitNormal.z:F2}";
                }
                
                // Display text next to hit point
                Handles.Label(hitPoint + Vector3.up * sphereRadius * 2, infoText, style);
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(HoverTerrainCursorDebug))]
public class HoverTerrainCursorDebugEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("This script shows cursor position when hovering over objects in the Scene view. It updates in real-time as you move your mouse.", MessageType.Info);
        
        EditorGUILayout.Space();
        if (GUILayout.Button("Focus Camera on This GameObject"))
        {
            if (Selection.activeGameObject != null)
            {
                SceneView.lastActiveSceneView.FrameSelected();
            }
        }
    }
}
#endif