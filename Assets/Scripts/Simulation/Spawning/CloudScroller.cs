// Updated CloudScroller class
using UnityEngine;

public class CloudScroller : MonoBehaviour
{
    public float scrollSpeed = 0.0001f;
    [HideInInspector] public Material materialInstance;
    private Vector2 offset = Vector2.zero;

    void Update()
    {
        if (materialInstance != null)
        {
            offset.x += Time.deltaTime * scrollSpeed;
            offset.y += Time.deltaTime * scrollSpeed * 0.5f;
            materialInstance.mainTextureOffset = offset;
        }
    }
}