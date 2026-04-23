using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ArgusTerminalPanel : MonoBehaviour
{
    [Header("状态容器")]
    public GameObject idleState;
    public GameObject downloadingState;
    public GameObject textDisplayState;

    [Header("顶部状态栏文字")]
    public TextMeshProUGUI statusBarText;  // 所有状态共用一个顶部文字

    [Header("下载状态 - 动态文字效果")]
    public TextMeshProUGUI downloadingBodyText;  // 下载时显示的正文区域（可选，用于逐字效果）

    [Header("完成状态 - 滚动文本")]
    public ScrollRect scrollRect;
    public TextMeshProUGUI terminalText;

    [Header("打字机效果设置")]
    public float typingSpeed = 0.02f;  // 每个字符间隔
    public float statusUpdateInterval = 0.15f;  // 状态栏数值刷新间隔

    private Coroutine downloadCoroutine;
    private string fullRevealText;
    private bool isDownloading = false;

    // 在类中添加
    public UnityEvent onTypingCompleted; // 逐字完成时触发

    private void Start()
    {
        // 初始待机状态
        ShowIdle();
    }

    /// <summary>
    /// 显示待机状态
    /// </summary>
    public void ShowIdle()
    {
        idleState.SetActive(true);
        downloadingState.SetActive(false);
        textDisplayState.SetActive(false);

        if (statusBarText != null)
            statusBarText.text = "> STANDBY_";
    }

    /// <summary>
    /// 开始下载并显示源码（由 GameManager 调用）
    /// </summary>
    public void StartDownloadAndDisplay(string text)
    {
        fullRevealText = text;

        if (downloadCoroutine != null)
            StopCoroutine(downloadCoroutine);

        downloadCoroutine = StartCoroutine(DownloadRoutine());
    }

    private IEnumerator DownloadRoutine()
    {
        isDownloading = true;

        // 切换到下载状态
        idleState.SetActive(false);
        downloadingState.SetActive(true);
        textDisplayState.SetActive(false);

        // 清空正文
        if (downloadingBodyText != null)
            downloadingBodyText.text = "";

        // 启动状态栏动态文字协程
        Coroutine statusRoutine = StartCoroutine(UpdateDownloadStatus());

        // 模拟打字机效果逐字显示正文
        string currentText = "";
        for (int i = 0; i < fullRevealText.Length; i++)
        {
            currentText += fullRevealText[i];
            if (downloadingBodyText != null)
                downloadingBodyText.text = currentText;
            yield return new WaitForSeconds(typingSpeed); // 使用固定的打字速度
        }

        // 停止状态栏刷新
        if (statusRoutine != null)
            StopCoroutine(statusRoutine);

        isDownloading = false;

        // 切换到完成状态
        downloadingState.SetActive(false);
        textDisplayState.SetActive(true);

        if (statusBarText != null)
            statusBarText.text = $"> DOWNLOADED // {fullRevealText.Length} BYTES";

        if (terminalText != null)
            terminalText.text = fullRevealText;

        Canvas.ForceUpdateCanvases();
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 1f;
        // 测试
        if (string.IsNullOrEmpty(fullRevealText))
        {
            Debug.LogWarning("ATP: fullRevealText is empty, invoking completed event immediately.");
            onTypingCompleted?.Invoke();
            yield break;
        }

        // *** 关键：触发完成事件 ***
        onTypingCompleted?.Invoke();
    }

    /// <summary>
    /// 动态刷新状态栏的下载速度
    /// </summary>
    private IEnumerator UpdateDownloadStatus()
    {
        System.Random rand = new System.Random();
        while (isDownloading)
        {
            int speed = rand.Next(100, 999);
            if (statusBarText != null)
                statusBarText.text = $"> DOWNLOADING... SPEED: {speed} BPMS";
            yield return new WaitForSeconds(statusUpdateInterval);
        }
    }

    /// <summary>
    /// 重置为待机状态（由 GameManager 调用）
    /// </summary>
    public void ResetToIdle()
    {
        if (downloadCoroutine != null)
        {
            StopCoroutine(downloadCoroutine);
            downloadCoroutine = null;
        }
        isDownloading = false;
        ShowIdle();
    }
}