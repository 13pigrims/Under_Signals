using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(ScrollRect))]
public class ScrollableText : MonoBehaviour
{
    [Header("组件引用")]
    public TextMeshProUGUI textComponent;
    public ScrollRect scrollRect;

    private void Reset()
    {
        scrollRect = GetComponent<ScrollRect>();
        if (scrollRect != null && scrollRect.content != null)
            textComponent = scrollRect.content.GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetText(string content)
    {
        if (textComponent != null)
            textComponent.text = content;
        Canvas.ForceUpdateCanvases();
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 1f;
    }

    public void AppendText(string content)
    {
        if (textComponent != null)
            textComponent.text += content;
        Canvas.ForceUpdateCanvases();
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 0f;
    }

    public void Clear()
    {
        if (textComponent != null)
            textComponent.text = "";
    }

    public void ScrollToBottom()
    {
        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}