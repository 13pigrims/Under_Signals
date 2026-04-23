using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DecodeReportPanel : MonoBehaviour
{
    [Header("输入控件")]
    public TMP_InputField hullIdInput;
    public TMP_Dropdown statusDropdown;
    public TMP_Dropdown cargoDropdown;
    public TMP_Dropdown intentDropdown;

    [Header("按钮")]
    public Button submitButton;
    public Button rejectButton;

    [Header("状态容器")]
    public GameObject normalState;
    public GameObject loadingState;

    [Header("加载动画")]
    public Image loadingSpinner;
    public Sprite[] loadingFrames;

    [Header("加载动画持续时间")]
    public float loadingDuration = 1.5f;   // 新增：可配置的加载时长

    private Coroutine loadingCoroutine;
    private bool canInteract = false;

    private void Start()
    {
        submitButton.onClick.AddListener(() => SubmitDecision(true));
        rejectButton.onClick.AddListener(() => SubmitDecision(false));
        SetupDropdowns();

        normalState.SetActive(true);
        loadingState.SetActive(false);
        SetButtonsInteractable(false);
    }

    void SetupDropdowns()
    {
        statusDropdown.ClearOptions();
        statusDropdown.AddOptions(new List<string> { "NOMINAL", "DAMAGE", "MEDICAL" });

        cargoDropdown.ClearOptions();
        cargoDropdown.AddOptions(new List<string> { "ORE", "FUEL", "FOOD", "EMPTY" });

        intentDropdown.ClearOptions();
        intentDropdown.AddOptions(new List<string> { "RESUPPLY", "OFFLOAD", "EMERGENCY" });
    }

    public void EnableInput()
    {
        canInteract = true;
        SetButtonsInteractable(true);
    }

    public void DisableInput()
    {
        canInteract = false;
        SetButtonsInteractable(false);
    }

    private void SetButtonsInteractable(bool interactable)
    {
        if (submitButton != null) submitButton.interactable = interactable;
        if (rejectButton != null) rejectButton.interactable = interactable;
    }

    void SubmitDecision(bool isApproved)
    {
        if (!canInteract)
        {
            Debug.LogWarning("DecodePanel: 当前不可提交");
            return;
        }

        SetButtonsInteractable(false);
        canInteract = false;

        PlayerAction action = new PlayerAction();
        action.submittedHullId = hullIdInput.text;
        action.statusIndex = statusDropdown.value;
        action.cargoIndex = cargoDropdown.value;
        action.intentIndex = intentDropdown.value;
        action.isApproved = isApproved;

        // 切换到加载状态
        normalState.SetActive(false);
        loadingState.SetActive(true);

        // 启动加载动画
        if (loadingCoroutine != null)
            StopCoroutine(loadingCoroutine);
        loadingCoroutine = StartCoroutine(PlayLoadingAnimation());

        // 通知 GameManager 处理数据
        GameManager.Instance.ProcessPlayerDecision(action);

        // 延迟重置 UI，保证加载动画可见
        StartCoroutine(DelayedReset(loadingDuration));
        Debug.Log("loadingState active: " + loadingState.activeSelf);
    }

    private IEnumerator DelayedReset(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetUI();
    }

    private IEnumerator PlayLoadingAnimation()
    {
        if (loadingFrames == null || loadingFrames.Length == 0)
        {
            Debug.LogWarning("DecodePanel: loadingFrames 为空");
            yield break;
        }

        int frame = 0;
        while (loadingState != null && loadingState.activeSelf)
        {
            if (loadingSpinner != null)
                loadingSpinner.sprite = loadingFrames[frame % loadingFrames.Length];
            frame++;
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void ResetUI()
    {
        // 停止加载协程
        if (loadingCoroutine != null)
        {
            StopCoroutine(loadingCoroutine);
            loadingCoroutine = null;
        }

        // 切换回填写界面
        if (normalState != null) normalState.SetActive(true);
        if (loadingState != null) loadingState.SetActive(false);

        // 清空输入
        if (hullIdInput != null) hullIdInput.text = "";
        if (statusDropdown != null) statusDropdown.value = 0;
        if (cargoDropdown != null) cargoDropdown.value = 0;
        if (intentDropdown != null) intentDropdown.value = 0;

        // 按钮状态仍由外部控制，此处不自动启用
    }
}