using UnityEngine;

public static class CarColorUtils
{
    public static void SetCarColor(Transform carRoot, Color color)
    {
        foreach (Transform child in carRoot)
        {
            Renderer renderer = child.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = color;
            }

            // Recursively apply to children
            if (child.childCount > 0)
            {
                SetCarColor(child, color);
            }
        }
    }
}