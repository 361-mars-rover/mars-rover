using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[ExecuteInEditMode]
public class SceneViewDimensions : MonoBehaviour
{
    [Header("Measurement Settings")]
    public bool showDimensionsInSceneView = true;
    public Color dimensionsColor = Color.yellow;
    public bool showWhenSelected = true;
    public bool showAlways = false;
    
    private Vector3 objectDimensions;
    private Renderer objectRenderer;
    private Collider objectCollider;

    void OnEnable()
    {
        // Subscribe to scene view update
        #if UNITY_EDITOR
        SceneView.duringSceneGui += OnSceneGUI;
        #endif
        
        MeasureObject();
    }

    void OnDisable()
    {
        // Unsubscribe from scene view update
        #if UNITY_EDITOR
        SceneView.duringSceneGui -= OnSceneGUI;
        #endif
    }

    void Update()
    {
        if (!Application.isPlaying)
        {
            MeasureObject();
        }
    }

    void MeasureObject()
    {
        objectRenderer = GetComponent<Renderer>();
        objectCollider = GetComponent<Collider>();
        
        if (objectRenderer != null)
        {
            objectDimensions = objectRenderer.bounds.size;
        }
        else if (objectCollider != null)
        {
            objectDimensions = objectCollider.bounds.size;
        }
        else
        {
            // If no renderer or collider, use the local scale multiplied by any parent scales
            objectDimensions = transform.lossyScale;
        }
    }
    
    #if UNITY_EDITOR
    void OnSceneGUI(SceneView sceneView)
    {
        if (!showDimensionsInSceneView)
            return;
            
        // Only show measurements when object is selected or if showAlways is true
        if ((showWhenSelected && !Selection.Contains(gameObject.GetInstanceID())) && !showAlways)
            return;
            
        // Get the center of the object
        Vector3 objectCenter = transform.position;
        if (objectRenderer != null)
            objectCenter = objectRenderer.bounds.center;
        else if (objectCollider != null)
            objectCenter = objectCollider.bounds.center;
            
        // Draw a label in the scene view
        Handles.color = dimensionsColor;
        
        string sourceText = objectRenderer != null ? "(Renderer)" : 
                          objectCollider != null ? "(Collider)" : "(Transform)";
                          
        string dimensionsText = gameObject.name + " " + sourceText + "\n" +
                               "Width: " + objectDimensions.x.ToString("F4") + "\n" +
                               "Height: " + objectDimensions.y.ToString("F4") + "\n" +
                               "Depth: " + objectDimensions.z.ToString("F4");
        
        Handles.Label(objectCenter, dimensionsText, EditorStyles.whiteBoldLabel);
        
        // Draw dimension lines to visualize the size
        Vector3 extents = objectDimensions * 0.5f;
        
        // Adjust position to where the bounds actually are
        Vector3 boundsCenter = objectCenter;
        
        // Draw dimension visualization lines
        DrawDimensionLines(boundsCenter, extents);
    }
    
    private void DrawDimensionLines(Vector3 center, Vector3 extents)
    {
        Color originalColor = Handles.color;
        Handles.color = dimensionsColor;
        
        // Draw width line (X-axis)
        Handles.DrawLine(center + new Vector3(-extents.x, -extents.y, -extents.z), 
                         center + new Vector3(extents.x, -extents.y, -extents.z));
        
        // Draw height line (Y-axis)
        Handles.DrawLine(center + new Vector3(-extents.x, -extents.y, -extents.z), 
                         center + new Vector3(-extents.x, extents.y, -extents.z));
                         
        // Draw depth line (Z-axis)
        Handles.DrawLine(center + new Vector3(-extents.x, -extents.y, -extents.z), 
                         center + new Vector3(-extents.x, -extents.y, extents.z));
        
        // Restore original color
        Handles.color = originalColor;
    }
    #endif
}
#endif