using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum QuadPanelState
{
    IdleScan,           // 状态一：待机持续检测
    ReadyToDownload,    // 状态二：显示下载按钮
    Downloading,        // 状态三：加载动画
    Displaying,         // 状态四：显示四象限和中心图标
    Processing          // 提交后等待反馈
}

public class QuadPanel : MonoBehaviour
{
    [Header("UI 容器")]
    public GameObject idleScanElements;
    public GameObject downloadButton;
    public GameObject downloadingElements;
    public GameObject displayElements;

    [Header("时间设置")]
    public float resetDuration = 1f; // 处理状态时长


    [Header("图片组件")]
    public Image centerIcon;
    public Image morseImage;
    public Image colorsImage;
    public Image shapesImage;
    public Image waveformImage;

    [Header("加载动画")]
    public Image loadingSpinner;          // 用于播放帧动画的 Image 组件
    public Sprite[] loadingFrames;        // 10 张序列帧

    public UnityEvent onDownloadStarted;

    public QuadPanelState CurrentState { get; private set; }

    private ShipData currentShip;
    // 添加一个标志，表示是否正在等待 ATP 完成
    private bool isWaitingForATP = false;

    private void Start()
    {
        CurrentState = QuadPanelState.IdleScan;
        UpdateUIForState();
    }

    // ========== 由 GameManager 调用的公开方法 ==========

    /// <summary>
    /// 进入状态一：待机扫描
    /// </summary>
    public void StartIdleScan()
    {
        CurrentState = QuadPanelState.IdleScan;
        UpdateUIForState();
    }

    /// <summary>
    /// 进入状态二：显示下载按钮
    /// </summary>
    public void SetReadyToDownload()
    {
        CurrentState = QuadPanelState.ReadyToDownload;
        UpdateUIForState();
    }

    public void SetCurrentShip(ShipData ship)
    {
        currentShip = ship;
    }

    /// <summary>
    /// 玩家提交决策后，进入处理状态并自动返回
    /// </summary>
    public void OnDecisionSubmitted()
    {
        StartCoroutine(ResetToIdleAfterDelay(resetDuration));
    }

    // ========== 按钮事件 ==========

    /// <summary>
    /// 下载按钮点击（需在 Inspector 中绑定）
    /// </summary>
    public void OnDownloadClicked()
    {
        if (CurrentState != QuadPanelState.ReadyToDownload) return;
        onDownloadStarted?.Invoke();
        StartCoroutine(DownloadRoutine());
    }

    public void OnATPCompleted()
    {
        isWaitingForATP = false;
    }

    // ========== 私有协程 ==========

    private IEnumerator DownloadRoutine()
    {
        CurrentState = QuadPanelState.Downloading;
        UpdateUIForState();

        isWaitingForATP = true;

        int frame = 0;
        // 持续播放动画，直到 isWaitingForATP 被设为 false
        while (isWaitingForATP)
        {
            if (loadingFrames != null && loadingFrames.Length > 0)
            {
                loadingSpinner.sprite = loadingFrames[frame % loadingFrames.Length];
                frame++;
            }
            yield return new WaitForSeconds(0.1f);
        }

        // 收到停止信号后，显示状态四
        if (currentShip != null)
            DisplayShipData(currentShip);
        else
            Debug.LogError("QuadPanel: currentShip 为空！");
    }


    IEnumerator ResetToIdleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        CurrentState = QuadPanelState.IdleScan;
        UpdateUIForState();
        GameManager.Instance.OnQuadPanelReadyForNextShip();
    }

    // ========== 显示飞船数据 ==========

    public void DisplayShipData(ShipData ship)
    {
        currentShip = ship;

        centerIcon.sprite = Resources.Load<Sprite>(ship.centerIconImage);
        morseImage.sprite = Resources.Load<Sprite>(ship.morseImage);
        colorsImage.sprite = Resources.Load<Sprite>(ship.colorsImage);
        shapesImage.sprite = Resources.Load<Sprite>(ship.shapesImage);
        waveformImage.sprite = Resources.Load<Sprite>(ship.waveformImage);

        CurrentState = QuadPanelState.Displaying;
        UpdateUIForState();
    }

    // ========== UI 状态切换 ==========

    private void UpdateUIForState()
    {
        // 先全部关闭
        if (idleScanElements != null) idleScanElements.SetActive(false);
        if (downloadButton != null) downloadButton.SetActive(false);
        if (downloadingElements != null) downloadingElements.SetActive(false);
        if (displayElements != null) displayElements.SetActive(false);

        // 根据当前状态开启对应的容器
        switch (CurrentState)
        {
            case QuadPanelState.IdleScan:
                if (idleScanElements != null) idleScanElements.SetActive(true);
                break;

            case QuadPanelState.ReadyToDownload:
                if (idleScanElements != null) idleScanElements.SetActive(true);
                if (downloadButton != null) downloadButton.SetActive(true);
                break;

            case QuadPanelState.Downloading:
                // 下载时保留扫描背景，显示下载动画容器
                if (idleScanElements != null) idleScanElements.SetActive(true);
                if (downloadingElements != null) downloadingElements.SetActive(true);
                break;

            case QuadPanelState.Displaying:
                if (displayElements != null) displayElements.SetActive(true);
                break;

            case QuadPanelState.Processing:
                // 处理状态保持显示，可以在此添加额外遮罩
                if (displayElements != null) displayElements.SetActive(true);
                break;
        }
    }
}