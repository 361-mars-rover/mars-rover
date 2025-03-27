using UnityEngine;
using UnityEngine.UI;

public class RoverViewSwitcher : MonoBehaviour
{
    public Camera[] roverCameras; // Array to hold references to the rover cameras
    public Button[] viewButtons; // Array to hold references to the view buttons

    void Start()
    {
        // Assign button click listeners
        for (int i = 0; i < viewButtons.Length; i++)
        {
            int index = i;  
            viewButtons[index].onClick.AddListener(() => SwitchCamera(index));
        }

        // Disable all cameras at start except the first one
        foreach (var cam in roverCameras)
        {
            cam.gameObject.SetActive(false);
        }
        if (roverCameras.Length > 0)
        {
            roverCameras[0].gameObject.SetActive(true);
        }
    }

    void SwitchCamera(int index)
    {
        // Disable all cameras
        foreach (var cam in roverCameras)
        {
            cam.gameObject.SetActive(false);
        }

        // Enable the selected camera
        roverCameras[index].gameObject.SetActive(true);
    }
}
