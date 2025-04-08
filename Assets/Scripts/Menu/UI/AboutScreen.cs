using UnityEngine;
using UnityEngine.UI;

public class AboutScreen: MonoBehaviour
{
    public GameObject[] slides; // Assign 6 slides in the inspector
    public Button nextButton;
    public Button backButton;

    private int currentSlideIndex = 0;

    void Start()
    {
        ShowSlide(currentSlideIndex);
        nextButton.onClick.AddListener(NextSlide);
        backButton.onClick.AddListener(PreviousSlide);

    }

    void ShowSlide(int index)
    {
        for (int i = 0; i < slides.Length; i++)
            slides[i].SetActive(i == index);

        backButton.gameObject.SetActive(index > 0);
        nextButton.gameObject.SetActive(index < slides.Length - 1);
    }

    void NextSlide()
    {
        if (currentSlideIndex < slides.Length - 1)
        {
            currentSlideIndex++;
            ShowSlide(currentSlideIndex);
        }
    }

    void PreviousSlide()
    {
        if (currentSlideIndex > 0)
        {
            currentSlideIndex--;
            ShowSlide(currentSlideIndex);
        }
    }
}