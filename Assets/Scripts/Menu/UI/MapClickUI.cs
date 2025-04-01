using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapClickUI : MonoBehaviour, IPointerClickHandler
{
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Convert the screen click position to a local position within the map
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out localPoint))
        {

            // Get the dimensions of the RectTransform
            float width = rectTransform.rect.width;
            float height = rectTransform.rect.height;


            // You can also calculate the normalized click position (0 to 1)
            float normalizedX = (localPoint.x + (width * 0.5f)) / width;
            float normalizedY = 1 - ((localPoint.y + (height * 0.5f)) / height);
            int tile_col = (int) (normalizedX * 256);
            int tile_row = (int) (normalizedY * 128);

            Debug.Log("Normalized click position - X: " + normalizedX + ", Y: " + normalizedY);
            Debug.Log("Normalized click position - col: " + tile_col + ", row: " + tile_row);
        }
    }
}
