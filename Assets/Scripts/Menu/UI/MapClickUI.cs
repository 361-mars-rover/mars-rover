using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;  // Add this line to fix the List<> error

public class MapClickUI : MonoBehaviour, IPointerClickHandler
{
    private RectTransform rectTransform;
    private int currentAvatarIndex = 0;
    private List<Avatar> avatars;
    
    public AvatarTableManager avatarTableManager;

    public GameObject pinPrefab;


    void Awake()
    {
        SetTopText($"Select spawn point for avatar {currentAvatarIndex}");
        rectTransform = GetComponent<RectTransform>();
    }

    public void UndoOnClick(){
        Debug.Log("Clicked undo!");
        if (currentAvatarIndex > 0) {
            currentAvatarIndex--;
            
            // Find the marker for the current avatar index and destroy it
            GameObject marker = GameObject.Find($"AvatarMarker_{currentAvatarIndex}");
            if (marker != null) {
                Destroy(marker);
            }
            
            // Reset that avatar's spawn position
            if (currentAvatarIndex < avatars.Count) {
                avatars[currentAvatarIndex].SpawnRowCol = new Vector2Int(-1, -1); // Reset to invalid position
            }
            SetTopText($"Select spawn point for rover {currentAvatarIndex}");
        }
    }

    void Start()
    {
        // Get the list of avatars from AvatarTableManager
        avatars = avatarTableManager.getAvatars();
        Debug.Log($"Ready to assign spawn positions for {avatars.Count} avatars.");
    }

    private void SetTopText(string newText){
        GameObject textObject = GameObject.Find("Select Spawn Point Text");
        // Get the TMP_Text component
        textObject.GetComponent<TMP_Text>().text = newText;
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
            
            // Create parent GameObject to hold both dot and text
            GameObject markerGroup = new GameObject($"AvatarMarker_{currentAvatarIndex}");
            markerGroup.transform.SetParent(rectTransform, false);
            RectTransform markerRect = markerGroup.AddComponent<RectTransform>();
            markerRect.anchoredPosition = localPoint;
            
            // Create dot as child of marker group
            GameObject dot = new GameObject("Dot");
            dot.transform.SetParent(markerGroup.transform, false);
            
            // Add a RectTransform and set its size
            RectTransform dotRect = dot.AddComponent<RectTransform>();
            dotRect.sizeDelta = new Vector2(15, 15);
            dotRect.anchoredPosition = Vector2.zero; // Center in parent
            
            // Add an Image component and set its color
            Image dotImage = dot.AddComponent<Image>();
            dotImage.color = Color.red;
            
            // Create text above the dot
            GameObject textObj = new GameObject("Label");
            textObj.transform.SetParent(markerGroup.transform, false);

            // Add TextMeshProUGUI component instead of Text
            TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
            tmpText.text = $"Avatar {currentAvatarIndex}";
            tmpText.fontSize = 24;
            tmpText.color = Color.white;
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.fontStyle = FontStyles.Bold;
            tmpText.outlineWidth = 0.2f;
            tmpText.outlineColor = Color.black;

            // Position text above dot
            RectTransform textRect = tmpText.GetComponent<RectTransform>();
            textRect.anchoredPosition = new Vector2(0, 20);
            textRect.sizeDelta = new Vector2(150, 30);
            
            // Bring the marker group to the front
            markerGroup.transform.SetAsLastSibling();
            
            currentAvatarIndex++;
            SetTopText($"Select spawn point for avatar {currentAvatarIndex}");
            
            // Check if all avatars are assigned
            if (currentAvatarIndex >= avatars.Count)
                SetTopText($"Click start simulation to begin!");
                Debug.Log("All spawn positions assigned!");
        }
            }
}