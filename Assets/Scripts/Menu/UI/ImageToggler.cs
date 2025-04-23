using UnityEngine;
using UnityEngine.UI;

/*
JIKAEL
A class to toggle the mineral data on the map
*/
public class ImageToggler : MonoBehaviour
{
    [SerializeField] private Sprite marsImage;
    [SerializeField] private Sprite alternateImage;
    [SerializeField] private Image targetImage;
    
    private bool showingMarsImage = true;
    
    void Update()
    {
        // Check for spacebar press
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleImage();
        }
    }
    
    private void ToggleImage()
    {
        showingMarsImage = !showingMarsImage;
        targetImage.sprite = showingMarsImage ? marsImage : alternateImage;
    }
}