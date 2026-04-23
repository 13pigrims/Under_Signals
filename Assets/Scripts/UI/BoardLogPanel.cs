using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using UnityEngine.InputSystem;

/// <summary>
/// 管理 SOP 手册窗口和 TRANSCRIPT LOG 窗口
/// </summary>
public class BoardLogPanel : MonoBehaviour
{
    [Header("按钮")]
    public Button sopButton;
    public Button logButton;

    [Header("窗口")]
    public GameObject sopWindow;
    public GameObject logWindow;

    [Header("窗口内的关闭按钮")]
    public Button sopCloseButton;
    public Button logCloseButton;

    [Header("窗口内的 ScrollableText")]
    public ScrollableText sopScrollableText;
    public ScrollableText logScrollableText;

    [Header("SOP 静态文本")]
    [TextArea(5, 20)]
    public string sopContent;

    private StringBuilder logBuilder = new StringBuilder();

    private void Start()
    {
        // 绑定打开窗口按钮
        if (sopButton != null)
            sopButton.onClick.AddListener(OpenSopWindow);
        if (logButton != null)
            logButton.onClick.AddListener(OpenLogWindow);

        // 绑定关闭窗口按钮
        if (sopCloseButton != null)
            sopCloseButton.onClick.AddListener(CloseSopWindow);
        if (logCloseButton != null)
            logCloseButton.onClick.AddListener(CloseLogWindow);

        // 初始化 SOP 文本
        if (sopScrollableText != null)
            sopScrollableText.SetText(sopContent);

        // 默认关闭窗口
        CloseAllWindows();
    }

    private void Update()
    {
        // 按 ESC 关闭所有窗口
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseAllWindows();
        }
    }

    // ---------- 窗口控制 ----------
    public void OpenSopWindow()
    {
        // （可选）打开 SOP 时自动关闭 LOG，避免重叠
        // CloseLogWindow();

        if (sopWindow != null)
            sopWindow.SetActive(true);
    }

    public void CloseSopWindow()
    {
        if (sopWindow != null)
            sopWindow.SetActive(false);
    }

    public void OpenLogWindow()
    {
        // （可选）打开 LOG 时自动关闭 SOP
        // CloseSopWindow();

        if (logWindow != null)
            logWindow.SetActive(true);
    }

    public void CloseLogWindow()
    {
        if (logWindow != null)
            logWindow.SetActive(false);
    }

    public void ToggleSopWindow()
    {
        if (sopWindow != null)
            sopWindow.SetActive(!sopWindow.activeSelf);
    }

    public void ToggleLogWindow()
    {
        if (logWindow != null)
            logWindow.SetActive(!logWindow.activeSelf);
    }

    public void CloseAllWindows()
    {
        CloseSopWindow();
        CloseLogWindow();
    }

    // ---------- 日志功能 ----------
    /// <summary>
    /// 添加一条日志记录（自动换行）
    /// </summary>
    public void AddLogEntry(string entry)
    {
        if (string.IsNullOrEmpty(entry))
            return;

        logBuilder.AppendLine(entry);
        UpdateLogText();
    }

    /// <summary>
    /// 清空所有日志
    /// </summary>
    public void ClearLog()
    {
        logBuilder.Clear();
        UpdateLogText();
    }

    /// <summary>
    /// 获取当前日志的完整文本
    /// </summary>
    public string GetLogText()
    {
        return logBuilder.ToString();
    }

    // 更新 LOG 窗口的文本并滚动到底部
    private void UpdateLogText()
    {
        if (logScrollableText != null)
        {
            logScrollableText.SetText(logBuilder.ToString());
            // SetText 内部已调用 Canvas.ForceUpdateCanvases，这里额外确保滚动到底部
            // 如果你的 ScrollableText 默认滚动到顶部，可以在此强制滚动到底部
            logScrollableText.ScrollToBottom();
        }
    }

    // ---------- SOP 内容 ----------
    public void SetSopContent(string content)
    {
        sopContent = content;
        if (sopScrollableText != null)
            sopScrollableText.SetText(content);
    }
}