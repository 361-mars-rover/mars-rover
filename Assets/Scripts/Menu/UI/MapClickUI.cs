using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;  // Add this line to fix the List<> error

public class MapClickUI : MonoBehaviour, IPointerClickHandler
{
    private RectTransform rectTransform;
    private int currentAvatarIndex = 0;
    private List<Avatar> avatars;
    
    public AvatarTableManager avatarTableManager;

    public GameObject pinPrefab;


    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Start()
    {
        // Get the list of avatars from AvatarTableManager
        avatars = avatarTableManager.getAvatars();
        Debug.Log($"Ready to assign spawn positions for {avatars.Count} avatars.");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Skip if all avatars have positions
        if (currentAvatarIndex >= avatars.Count)
            return;

        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, 
            eventData.position, 
            eventData.pressEventCamera, 
            out localPoint))
        {
            // Calculate row and column
            float width = rectTransform.rect.width;
            float height = rectTransform.rect.height;

            float normalizedX = (localPoint.x + (width * 0.5f)) / width;
            float normalizedY = 1 - ((localPoint.y + (height * 0.5f)) / height);
            int tile_col = (int)(normalizedX * 256);
            int tile_row = (int)(normalizedY * 128);

            // Assign to current avatar
            avatars[currentAvatarIndex].SpawnRowCol = new Vector2Int(tile_col, tile_row);
            //Debug.Log($"Assigned to Avatar {currentAvatarIndex}: (Row {tile_row}, Col {tile_col})");

            GameObject dot = new GameObject("Dot");
            dot.transform.SetParent(rectTransform, false);
            
            // Add a RectTransform and set its size
            RectTransform dotRect = dot.AddComponent<RectTransform>();
            dotRect.sizeDelta = new Vector2(20, 20);  // 10x10 dot

            // Add an Image component and set its color (or assign a sprite if you prefer)
            Image dotImage = dot.AddComponent<Image>();
            dotImage.color = Color.red;

            // Set the dot's anchored position to the localPoint
            dotRect.anchoredPosition = localPoint;

            // Bring the dot to the front
            dot.transform.SetAsLastSibling();


            currentAvatarIndex++;

            // Check if all avatars are assigned
            if (currentAvatarIndex >= avatars.Count)
                Debug.Log("All spawn positions assigned!");
        }
    }
}