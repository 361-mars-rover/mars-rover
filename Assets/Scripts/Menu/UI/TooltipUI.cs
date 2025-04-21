using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class TooltipUI : MonoBehaviour
{
    public static TooltipUI Instance;

    public RectTransform tooltipTransform;
    public TextMeshProUGUI tooltipText;
    public CanvasGroup canvasGroup;

    private void Awake()
    {
        Instance = this;
        HideTooltip();
    }

    private void Update()
    {
        if (canvasGroup.alpha > 0)
        {
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                transform.parent as RectTransform,
                Input.mousePosition,
                null,
                out pos
            );
            tooltipTransform.anchoredPosition = pos + new Vector2(10, -10);
        }
    }

    public void ShowTooltip(string message)
    {
        tooltipText.text = message;
        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;
    }

    public void HideTooltip()
    {
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
    }
}
