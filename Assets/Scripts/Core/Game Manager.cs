using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("数据")]
    public ShipDatabase shipDatabase;

    [Header("UI 面板引用")]
    public QuadPanel quadPanel;
    public DecodeReportPanel decodePanel;
    public ArgusTerminalPanel terminalPanel;
    public DailyArrivalsPanel arrivalsPanel;
    public BoardLogPanel logPanel;
    public UINarrativeManager narrativeManager;   // 新增：用于显示剧情
    public EndingPanel endingPanel;

    [Header("主菜单")]
    public GameObject mainMenuPanel;               // 主菜单面板，初始显示

    [Header("时间设置")]
    public float idleScanDuration = 3f;

    // 当日飞船队列
    private List<ShipData> todayShips;
    private int currentShipIndex = 0;
    private int currentDay = 1;

    // 决策状态
    private bool isDecisionSubmitted = false;
    private PlayerAction currentPlayerAction;

    // Mimic 统计
    private int totalMimicsAllowed = 0;
    private int totalMimicsDenied = 0;

    // 玩家历史操作记录
    private List<PlayerAction> dayActions = new List<PlayerAction>();

    // 当天是否已显示开场叙事
    private bool dayNarrativeShown = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // 绑定事件
        quadPanel.onDownloadStarted.AddListener(OnQuadDownloadStarted);
        terminalPanel.onTypingCompleted.AddListener(quadPanel.OnATPCompleted);

        // 初始显示主菜单，隐藏游戏主界面（可选）
        mainMenuPanel.SetActive(true);
        if (narrativeManager == null)
            Debug.LogError("GameManager: narrativeManager 未赋值！");
        // 游戏主 UI 可以暂时隐藏，或由主菜单遮罩覆盖
    }
    /// <summary>
    /// 检测到按下 ESC 键时退出全屏（如果处于全屏状态）
    /// </summary>
    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            Screen.fullScreenMode = FullScreenMode.Windowed;
    }

    /// <summary>
    /// 主菜单“开始游戏”按钮调用
    /// </summary>
    public void StartGame()
    {
        mainMenuPanel.SetActive(false);
        currentDay = 1;
        totalMimicsAllowed = 0;
        totalMimicsDenied = 0;
        LoadDay(currentDay);
    }

    // 同步下载状态
    void OnQuadDownloadStarted()
    {
        ShipData ship = todayShips[currentShipIndex];
        terminalPanel.StartDownloadAndDisplay(ship.terminalRevealText);
    }

    void LoadDay(int day)
    {
        todayShips = DataManager.Instance.GetShipsForDay(day);
        currentShipIndex = 0;
        dayActions.Clear();
        dayNarrativeShown = false;

        string manifestText = GetManifestText(day);
        arrivalsPanel.SetManifest(manifestText);

        DayNarrative narrative = shipDatabase.GetNarrative(day);
        if (narrative != null && !string.IsNullOrEmpty(narrative.narrativeOpen))
        {
            Debug.Log($"Showing narrative for day {day}: {narrative.narrativeOpen.Substring(0, 30)}...");
            narrativeManager.ShowNarrative(narrative.narrativeOpen, () =>
            {
                dayNarrativeShown = true;
                StartCoroutine(DayRoutine());
            });
        }
        else
        {
            Debug.LogWarning($"No narrative open for day {day}");
            dayNarrativeShown = true;
            StartCoroutine(DayRoutine());
        }
    }

    IEnumerator DayRoutine()
    {
        // 确保开场叙事已播完（如果有）
        while (!dayNarrativeShown)
            yield return null;

        while (currentShipIndex < todayShips.Count)
        {
            ShipData ship = todayShips[currentShipIndex];

            decodePanel.DisableInput();
            quadPanel.SetCurrentShip(ship);

            quadPanel.StartIdleScan();
            yield return new WaitForSeconds(idleScanDuration);

            quadPanel.SetReadyToDownload();
            yield return new WaitUntil(() => quadPanel.CurrentState == QuadPanelState.Downloading);
            yield return new WaitUntil(() => quadPanel.CurrentState == QuadPanelState.Displaying);

            decodePanel.EnableInput();

            isDecisionSubmitted = false;
            yield return new WaitUntil(() => isDecisionSubmitted);

            decodePanel.DisableInput();
            quadPanel.OnDecisionSubmitted();
            terminalPanel.ResetToIdle();
            decodePanel.ResetUI();

            currentShipIndex++;
        }

        EndDay();
    }

    public void ProcessPlayerDecision(PlayerAction action)
    {
        ShipData currentShip = todayShips[currentShipIndex];

        int correctStatusIndex = (int)currentShip.status;
        int correctCargoIndex = (int)currentShip.cargo;
        int correctIntentIndex = (int)currentShip.intent;

        action.isCorrect = (action.submittedHullId == currentShip.shipId &&
                            action.statusIndex == correctStatusIndex &&
                            action.cargoIndex == correctCargoIndex &&
                            action.intentIndex == correctIntentIndex);

        RecordConsequence(currentShip, action.isApproved, action.isCorrect);

        currentPlayerAction = action;
        dayActions.Add(action);

        string logEntry = $"{currentShip.shipId} | {(action.isApproved ? "APPROVED" : "REJECTED")} | {(action.isCorrect ? "✓" : "✗")}";
        logPanel.AddLogEntry(logEntry);

        isDecisionSubmitted = true;
    }

    private void RecordConsequence(ShipData ship, bool approved, bool isCorrect)
    {
        if (ship.isMimic)
        {
            if (approved) totalMimicsAllowed++;
            else totalMimicsDenied++;
        }

        Debug.Log($"[GameManager] {ship.shipId} — Approved:{approved} Correct:{isCorrect} | Mimic:{ship.isMimic} | Mimics allowed:{totalMimicsAllowed} denied:{totalMimicsDenied}");
    }

    private string GetManifestText(int day)
    {
        DayNarrative narrative = shipDatabase.GetNarrative(day);
        if (narrative != null && !string.IsNullOrEmpty(narrative.manifestDisplayText))
            return narrative.manifestDisplayText;
        return $"[NO MANIFEST DATA FOR DAY {day}]";
    }

    void EndDay()
    {
        Debug.Log($"GameManager: EndDay for day {currentDay}");
        Debug.Log($"Day {currentDay} complete. Processed {todayShips.Count} ships.");

        DayNarrative narrative = shipDatabase.GetNarrative(currentDay);
        string closeText = "";

        if (narrative != null)
        {
            bool caughtMimicToday = false;
            bool missedMimicToday = false;

            foreach (var action in dayActions)
            {
                ShipData ship = todayShips.Find(s => s.shipId == action.submittedHullId);
                if (ship != null && ship.isMimic)
                {
                    if (!action.isApproved) caughtMimicToday = true;
                    else missedMimicToday = true;
                }
            }

            if (caughtMimicToday && !string.IsNullOrEmpty(narrative.narrativeCloseCaught))
                closeText = narrative.narrativeCloseCaught;
            else if (missedMimicToday && !string.IsNullOrEmpty(narrative.narrativeCloseMissed))
                closeText = narrative.narrativeCloseMissed;
            else if (!string.IsNullOrEmpty(narrative.narrativeCloseDefault))
                closeText = narrative.narrativeCloseDefault;
        }

        if (!string.IsNullOrEmpty(closeText))
        {
            narrativeManager.ShowNarrative(closeText, () =>
            {
                ProceedAfterDayEnd();
            });
        }
        else
        {
            ProceedAfterDayEnd();
        }
    }

    void ProceedAfterDayEnd()
    {
        string report = GenerateDutyReport();
        logPanel.AddLogEntry("\n=== DUTY REPORT ===\n" + report);

        if (currentDay >= 5)
        {
            ShowEnding();
        }
        else
        {
            currentDay++;
            LoadDay(currentDay);
        }
    }

    private string GenerateDutyReport()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine($"DAY {currentDay} DUTY LOG");
        sb.AppendLine("----------------------------------------");
        foreach (var action in dayActions)
        {
            string decision = action.isApproved ? "CLEARED" : "REJECTED";
            string correct = action.isCorrect ? "CORRECT" : "ERROR";
            sb.AppendLine($"{action.submittedHullId} | {decision} | {correct}");
        }
        return sb.ToString();
    }

    private void ShowEnding()
    {
        string title;
        string body;

        if (totalMimicsAllowed >= 3)
        {
            title = "ENDING A: PORT COMPROMISED";
            body = "By the time the lockdown order came, it was already too late.\n\n" +
                "The ships that passed through your checkpoint — they weren't carrying cargo. They were carrying mimics.\n\n" +
                "They learned to forge ARGUS codes. They learned to simulate normal four-quadrant readings. And you failed to see through them.\n\n" +
                "The station descended into chaos within 48 hours.\n\n" +
                "The military's evacuation report listed you as \"critical negligence personnel.\" But you know the truth is more complicated —\n\n" +
                "Those redacted sections in the manual were supposed to help you identify them.\n\n" +
                "Someone didn't want you to know.\n\n" +
                "[ OLYMPUS STATION — ABANDONED ]";
        }
        else if (totalMimicsDenied >= 3)
        {
            title = "ENDING B: THE VIGILANT SENTINEL";
            body = "You did it.\n\n" +
                "Despite a half-redacted manual. Despite the military deliberately hiding the truth. You still found the cracks in their disguises.\n\n" +
                "The mimics — whatever they are — did not get past your checkpoint.\n\n" +
                "After the lockdown, the military found staggering evidence in the wreckage of the ships you denied:\n\n" +
                "The metal of those hulls was alive. They didn't ride the ships here — they WERE the ships.\n\n" +
                "Your judgment saved the entire station.\n\n" +
                "The chief gripped your hand before evacuation: \"Those redacted parts in the manual... I'm sorry. Orders from above.\"\n\n" +
                "\"I know,\" you said. \"But patterns can't be hidden.\"\n\n" +
                "[ OLYMPUS STATION — OPERATING NORMALLY ]";
        }
        else
        {
            title = "ENDING C: BLURRED LINES";
            body = "Some you let through. Some you stopped.\n\n" +
                "You can't be sure whether you made mistakes — the military's report is classified, and no one told you what happened to the ships you cleared.\n\n" +
                "The station didn't fall, but it's far from safe.\n\n" +
                "Sectors B and D remain under lockdown. Occasionally, someone reports hearing strange sounds from the ventilation shafts.\n\n" +
                "You still sit at your terminal every day, inspecting incoming ships.\n\n" +
                "You've worked out most of the redacted sections. But is \"most\" enough?\n\n" +
                "You don't know.\n\n" +
                "The mimics might still be out there. Or perhaps they're already inside.\n\n" +
                "[ OLYMPUS STATION — STATUS: UNKNOWN ]";
        }

        endingPanel.Show(title, body, totalMimicsAllowed, totalMimicsDenied, () =>
        {
            // Return to main menu
            mainMenuPanel.SetActive(true);
            quadPanel.StartIdleScan();
            decodePanel.ResetUI();
            terminalPanel.ResetToIdle();
        });
    }

    public void OnQuadPanelReadyForNextShip()
    {
        decodePanel.ResetUI();
        Debug.Log("QuadPanel ready for next ship.");
    }
}