using UnityEngine;
using UnityEngine.UI;

public class ScrollResetter : MonoBehaviour
{
    public ScrollRect scrollRect;

    void Start()
    {
        // Delay slightly to wait for layout to finish
        StartCoroutine(ScrollToTopNextFrame());
    }

    System.Collections.IEnumerator ScrollToTopNextFrame()
    {
        yield return null; // Wait for one frame
        scrollRect.verticalNormalizedPosition = 1f;
    }
}