using UnityEngine;

/// <summary>
/// 每日排班表面板
/// </summary>
public class DailyArrivalsPanel : MonoBehaviour
{
    [Header("内部组件")]
    public ScrollableText scrollableText;

    /// <summary>
    /// 设置排班表文本
    /// </summary>
    public void SetManifest(string text)
    {
        if (scrollableText != null)
            scrollableText.SetText(text);
    }

    public void Clear()
    {
        if (scrollableText != null)
            scrollableText.Clear();
    }
}